// -----------------------------------------------------------------------
// <copyright file="Mirror.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Ditto.Model;
    using Ditto.Properties;

    using log4net;

    public class Mirror
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Mirror));

        private string Source { get; set; }

        private string Destination { get; set; }

        private HashSet<string> FileExceptions { get; set; }

        private HashSet<string> FolderExceptions { get; set; }
        
        private HashSet<string> sourceFiles;

        private HashSet<string> sourceDirectories;

        private HashSet<string> destinationFiles;

        private HashSet<string> destinationDirectories;

        private readonly MirrorInfo mirrorInfo;

        /// <summary>
        /// Creates a new folder mirror instance.
        /// </summary>
        /// <param name="source">The source folder.</param>
        /// <param name="destination">The destination folder.</param>
        /// <param name="fileExceptions">Optional hashset of file exceptions.</param>
        /// <param name="folderExceptions">Optional hashset of folder exceptions.</param>
        public Mirror(string source, string destination, HashSet<string> fileExceptions = null, HashSet<string> folderExceptions = null)
        {
            this.Source = source;
            this.Destination = destination;
            this.FileExceptions = fileExceptions ?? new HashSet<string>();
            this.FolderExceptions = folderExceptions ?? new HashSet<string>();
            mirrorInfo = new MirrorInfo();
        }

        /// <summary>
        /// Starts the folder mirroring.
        /// </summary>
        /// <returns>A <see cref="MirrorInfo"/> object containing information about the mirror.</returns>
        public MirrorInfo Start()
        {
            Log.Info("Collecting Data");
            Task[] taskArray =
            {
                Task.Factory.StartNew(() => this.BuildFolderTree(
                    this.Source, 
                    out this.sourceFiles, 
                    out this.sourceDirectories, 
                    out this.mirrorInfo.SourceSize)),
                Task.Factory.StartNew(() => this.BuildFolderTree(
                    this.Destination, 
                    out this.destinationFiles, 
                    out this.destinationDirectories, 
                    out this.mirrorInfo.DestinationSize))
            };

            Task.WaitAll(taskArray);

            this.mirrorInfo.SourceFolders = this.sourceDirectories.Count;
            this.mirrorInfo.SourceFiles = this.sourceFiles.Count;
            this.mirrorInfo.DestinationFolders = this.sourceDirectories.Count;
            this.mirrorInfo.DestinationFiles = this.destinationFiles.Count;

            this.MirrorFileDeletions();
            this.MirrorFolderCreations();
            this.MirrorFolderDeletions();
            this.MirrorFiles();

            return this.mirrorInfo;
        }

        private void BuildFolderTree(string folder, out HashSet<string> fileList, out HashSet<string> directoryList, out long size)
        {
            fileList = new HashSet<string>();
            directoryList = new HashSet<string>();
            size = 0;

            var directories = new Stack<string>(20);

            // TODO: Check directory exists

            directories.Push(folder);

            while (directories.Count > 0)
            {
                var currentDirectory = directories.Pop();
                string[] subDirectories;

                try
                {
                    subDirectories = Directory.GetDirectories(currentDirectory);
                }
                catch (UnauthorizedAccessException)
                {
                    Log.ErrorFormat("Access denied to {0}", currentDirectory); // TODO: Add to resources
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    Log.ErrorFormat("Cannot find directory {0}", currentDirectory); // TODO: Add to resources
                    continue;
                }
                catch (PathTooLongException)
                {
                    Log.ErrorFormat("Directory path {0} is too long, skipping", currentDirectory);
                    continue;
                }
                
                string[] files;
                try
                {
                    files = Directory.GetFiles(currentDirectory);
                }
                catch (UnauthorizedAccessException)
                {
                    Log.ErrorFormat("Access denied to {0}", currentDirectory); // TODO: Add to resources
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    Log.ErrorFormat("Cannot find directory {0}", currentDirectory); // TODO: Add to resources
                    continue;
                }
                catch (PathTooLongException)
                {
                    Log.ErrorFormat("Directory path {0} is too long, skipping", currentDirectory);
                    continue;
                }

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);

                    if (!this.FileExceptions.Contains(fileInfo.Name))
                    {
                        fileList.Add(fileInfo.FullName);
                        size += fileInfo.Length;
                    }
                }

                foreach (var subDirectory in subDirectories)
                {
                    var directoryInfo = new DirectoryInfo(subDirectory);
                    if (!this.FolderExceptions.Contains(directoryInfo.Name))
                    {
                        directoryList.Add(directoryInfo.FullName);
                        directories.Push(subDirectory);
                    }
                }
            }
        }

        private void MirrorFileDeletions()
        {
            foreach (var destinationFile in this.destinationFiles)
            {
                if (sourceFiles.Contains(this.ReflectPath(destinationFile)))
                {
                    continue;
                }

                Log.InfoFormat(Resources.FileDeletion, destinationFile);
                try
                {
                    File.Delete(destinationFile);
                    this.mirrorInfo.FilesDeleted++;
                }
                catch (IOException)
                {
                    Log.ErrorFormat(Resources.FileDeletionIOError, destinationFile);
                    this.mirrorInfo.FileErrors++;
                }
                catch (UnauthorizedAccessException)
                {
                    Log.ErrorFormat(Resources.FileDeletionUnauthorized, destinationFile);
                    this.mirrorInfo.FileErrors++;
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat(Resources.FileDeletionUnknown, destinationFile, exception.Message);
                    this.mirrorInfo.FileErrors++;
                }
            }
        }

        private void MirrorFolderCreations()
        {
            // Folders in source but not in destination
            foreach (var sourceDirectory in this.sourceDirectories)
            {
                var destinationDirectory = this.ReflectPath(sourceDirectory);
                if (this.destinationDirectories.Contains(destinationDirectory))
                {
                    continue;
                }

                Log.InfoFormat(Resources.DirCreation, destinationDirectory);
                try
                {
                    Directory.CreateDirectory(destinationDirectory);
                    this.mirrorInfo.FoldersCreated++;
                }
                catch (UnauthorizedAccessException)
                {
                    Log.ErrorFormat(Resources.DirCreationUnauthorized, destinationDirectory);
                    this.mirrorInfo.FolderErrors++;
                }
                catch (PathTooLongException)
                {
                    Log.ErrorFormat(Resources.DirCreationLength, destinationDirectory);
                    this.mirrorInfo.FolderErrors++;
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat(Resources.DirCreationUnknown, destinationDirectory, exception.Message);
                    this.mirrorInfo.FolderErrors++;
                }
            }
        }

        private void MirrorFolderDeletions()
        {
            // Folders in destination but not in source
            foreach (var destinationDirectory in this.destinationDirectories.OrderByDescending(d => d))
            {
                if (this.sourceDirectories.Contains(this.ReflectPath(destinationDirectory)))
                {
                    continue;
                }

                Log.InfoFormat(Resources.DirDeletion, destinationDirectory);
                try
                {
                    Directory.Delete(destinationDirectory);
                    this.mirrorInfo.FoldersDeleted++;
                }
                catch (PathTooLongException)
                {
                    Log.ErrorFormat(Resources.DirDeletionLength, destinationDirectory);
                    this.mirrorInfo.FolderErrors++;
                }
                catch (IOException)
                {
                    Log.ErrorFormat(Resources.DirDeletionIOError, destinationDirectory);
                    this.mirrorInfo.FolderErrors++;
                }
                catch (UnauthorizedAccessException)
                {
                    Log.ErrorFormat(Resources.DirDeletionUnauthorized, destinationDirectory);
                    this.mirrorInfo.FolderErrors++;
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat(Resources.DirDeletionUnknown, destinationDirectory, exception.Message);
                    this.mirrorInfo.FolderErrors++;
                }
            }
        }

        private void MirrorFiles()
        {           
            foreach (var sourceFile in sourceFiles)
            {
                var sourceFileInfo = new FileInfo(sourceFile);
                var destinationFileInfo = new FileInfo(this.ReflectPath(sourceFileInfo.FullName));
                
                // Source file not in the destination
                if (!this.destinationFiles.Contains(destinationFileInfo.FullName))
                {
                    Log.InfoFormat(Resources.FileCopy, sourceFileInfo.FullName);
                    this.CopyFile(sourceFileInfo, destinationFileInfo);
                }
                // Source file is in the destination
                else if (this.destinationFiles.Contains(destinationFileInfo.FullName))
                {
                    var isEqual = new FileComparer(sourceFileInfo, destinationFileInfo).Compare();

                    if (isEqual)
                    {
                        Log.InfoFormat(Resources.FileSkip, sourceFileInfo.FullName);
                        this.mirrorInfo.FilesSkipped++;
                    }
                    else
                    {
                        Log.InfoFormat(Resources.FileUpdate, sourceFileInfo.FullName);
                        this.CopyFile(sourceFileInfo, destinationFileInfo, true);
                    }
                }
            }
        }

        private void CopyFile(FileInfo source, FileInfo destination, bool update = false)
        {
            try
            {
                if (update)
                {
                    File.Copy(source.FullName, destination.FullName, true);

                    destination.CreationTime = source.CreationTime;
                    destination.CreationTimeUtc = source.CreationTimeUtc;
                    destination.LastWriteTime = source.LastWriteTime;
                    destination.LastWriteTimeUtc = source.LastWriteTimeUtc;
                    destination.LastAccessTime = source.LastAccessTime;
                    destination.LastAccessTimeUtc = source.LastAccessTimeUtc;

                    this.mirrorInfo.FilesUpdated++;
                }
                else
                {
                    File.Copy(source.FullName, destination.FullName);

                    destination.CreationTime = source.CreationTime;
                    destination.CreationTimeUtc = source.CreationTimeUtc;
                    destination.LastWriteTime = source.LastWriteTime;
                    destination.LastWriteTimeUtc = source.LastWriteTimeUtc;
                    destination.LastAccessTime = source.LastAccessTime;
                    destination.LastAccessTimeUtc = source.LastAccessTimeUtc;

                    this.mirrorInfo.FilesCreated++;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log.ErrorFormat(Resources.FileCopyUnauthorized, source.FullName);
                this.mirrorInfo.FileErrors++;
            }
            catch (ArgumentException)
            {
                Log.ErrorFormat(Resources.FileCopyInvalidArg, source.FullName);
            }
            catch (PathTooLongException)
            {
                Log.ErrorFormat(Resources.FileCopyLength, source.FullName);
            }
            catch (DirectoryNotFoundException)
            {
                Log.ErrorFormat(Resources.FileCopyDirNotFound, source.FullName);
            }
            catch (IOException)
            {
                Log.ErrorFormat(Resources.FileCopyIOError, source.FullName);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat(Resources.FileCopyUnknown, source.FullName, exception.Message);
            }
        }

        private string ReflectPath(string path)
        {
            var reflectedPath = string.Empty;
            if (path.Contains(this.Source))
            {
                reflectedPath = path.Replace(this.Source, this.Destination);
            }
            else if (path.Contains(this.Destination))
            {
                reflectedPath = path.Replace(this.Destination, this.Source);
            }

            return reflectedPath;
        }
    }
}