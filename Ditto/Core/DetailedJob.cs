// -----------------------------------------------------------------------
// <copyright file="DetailedJob.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Ditto.Model;
    using Ditto.Properties;

    using log4net;

    using Newtonsoft.Json;

    public static class DetailedJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DetailedJob));

        private static string JobDetailsFile { get; set; }

        private static JobDetails JobDetails { get; set; }

        private static StringBuilder emailLog;

        public static void Start(string jobDetailsFile)
        {
            // Set required objects
            var stopwatch = new Stopwatch();
            emailLog = new StringBuilder();
            JobDetailsFile = jobDetailsFile;

            // Terminate if job is not valid
            if (!JobIsValid()) { return; }

            // Attempt to verify identity if identify set to true
            if (JobDetails.Identify)
            {
                IdentifyDestination();   
            }

            // Start stopwatch and begin mirroring sources
            stopwatch.Start();
            foreach (var jobSource in JobDetails.Sources)
            {
                MirrorSource(jobSource);
            }
            
            // Stop stopwatch and append to email report
            stopwatch.Stop();
            emailLog.AppendFormat("Total Time: {0} Hours, {1} Minutes", stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes);

            // Send email report if SMTP is present
            if (!string.IsNullOrWhiteSpace(JobDetails.Smtp) || JobDetails.Smtp != "")
            {
                new Emailer(JobDetails, emailLog.ToString()).Send();
            }
        }

        private static void MirrorSource(JobSource jobSource)
        {
            var fileExceptions = new HashSet<string>(jobSource.FileExceptions);
            var folderExceptions = new HashSet<string>(jobSource.FolderExceptions);
            var destinationPath = Path.Combine(JobDetails.Destination, jobSource.Destination);

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var mirror = new Mirror(jobSource.Source, destinationPath, fileExceptions, folderExceptions).Start();

            emailLog.AppendFormat("{0}: ", jobSource.Name).AppendLine();
            emailLog.AppendFormat(Resources.EmailFrom, jobSource.Source).AppendLine();
            emailLog.AppendFormat(Resources.EmailTo, destinationPath).AppendLine();
            emailLog.AppendFormat(Resources.EmailSource, mirror.SourceFolders, mirror.SourceFiles,mirror.SourceSizeString).AppendLine();
            emailLog.AppendFormat(Resources.EmailDestination, mirror.DestinationFolders, mirror.DestinationFiles, mirror.DestinationSizeString).AppendLine();
            emailLog.AppendFormat(Resources.FolderStats, mirror.FoldersCreated, mirror.FoldersDeleted, mirror.FoldersSkipped, mirror.FolderErrors).AppendLine();
            emailLog.AppendFormat(Resources.FileStats, mirror.FilesCreated, mirror.FilesDeleted, mirror.FilesUpdated, mirror.FilesSkipped, mirror.FileErrors).AppendLine().AppendLine();
        }

        private static bool JobIsValid()
        {
            bool isValid;

            var fileInfo = new FileInfo(JobDetailsFile);
            if (fileInfo.Exists)
            {
                isValid = ParseJson();
            }
            else
            {
                Log.Fatal("Job details file does not exist.");
                isValid = false;
            }

            if (isValid && !Directory.Exists(JobDetails.Destination))
            {
                Log.FatalFormat("Destination {0} could not be found", JobDetails.Destination);
                isValid = false;

                if (!string.IsNullOrWhiteSpace(JobDetails.Smtp) || JobDetails.Smtp != "")
                {
                    new Emailer(JobDetails, string.Format("Destination {0} could not be found", JobDetails.Destination)).Send();
                }
            }

            return isValid;
        }

        private static bool ParseJson()
        {
            var isValid = true;

            try
            {
                var jsonData = File.ReadAllText(JobDetailsFile);
                JobDetails = JsonConvert.DeserializeObject<JobDetails>(jsonData);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception.Message);
                isValid = false;
            }

            return isValid;
        }

        private static void IdentifyDestination()
        {
            var filePath = Path.Combine(JobDetails.Destination, "identity");
            var identityFile = new FileInfo(filePath);

            if (identityFile.Exists)
            {
                var identity = File.ReadLines(filePath).Take(1).SingleOrDefault();
                emailLog.AppendFormat("Destination identified as {0}", identity).AppendLine().AppendLine();
            }
            else
            {
                emailLog.AppendFormat("Failed to indentify destination").AppendLine().AppendLine();
            }
        }
    }
}