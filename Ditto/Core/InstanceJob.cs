// -----------------------------------------------------------------------
// <copyright file="InstanceJob.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Ditto.Properties;

    using log4net;

    public static class InstanceJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InstanceJob));

        private static string Source { get; set; }

        private static string Destination { get; set; }

        public static void Start(string source, string destination)
        {
            var stopwatch = new Stopwatch();
            Source = source;
            Destination = destination;

            if (!JobIsValid()) { return; }
            
            stopwatch.Start();
            var mirror = new Mirror(Source, Destination).Start();
            stopwatch.Stop();

            Log.InfoFormat("Job completed in {0}", stopwatch.Elapsed);
            Log.InfoFormat(Resources.FolderStats, mirror.FoldersCreated, mirror.FoldersDeleted, mirror.FoldersSkipped, mirror.FolderErrors);
            Log.InfoFormat(Resources.FileStats, mirror.FilesCreated, mirror.FilesDeleted, mirror.FilesUpdated, mirror.FilesSkipped, mirror.FileErrors);
            Console.Read();
        }

        private static bool JobIsValid()
        {
            var isValid = true;

            if (!Directory.Exists(Source))
            {
                Log.Fatal("Source path does not exist or is invalid.");
                isValid = false;
                Console.Read();
            }

            if (!Directory.Exists(Destination) && !isValid)
            {
                try
                {
                    Directory.CreateDirectory(Destination);
                }
                catch (Exception)
                {
                    Log.Fatal("Destination does not exist and unable to create it.");
                    isValid = false;
                    Console.Read();
                }
            }

            return isValid;
        }
    }
}