// -----------------------------------------------------------------------
// <copyright file="Maintenance.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System;
    using System.IO;

    public static class Maintenance
    {
        public static void CleanupLogs()
        {
            var lastWeek = DateTime.Today.AddDays(-7);
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            var dittoDirectory = Path.Combine(appData, "Ditto");
            var logs = Directory.GetFiles(dittoDirectory);
            
            foreach (var log in logs)
            {
                var fileInfo = new FileInfo(log);
                if (fileInfo.LastWriteTime < lastWeek)
                {
                    File.Delete(fileInfo.FullName);
                }
            }

        }
    }
}