// -----------------------------------------------------------------------
// <copyright file="MirrorInfo.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Ditto.Core;

    using log4net;
    using log4net.Config;

    class Loader
    {
        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetExecutingAssembly();
            }
        }

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                AbortOnError("No arguments specified.");
            }
            
            InitializeLog();

            if (args.Count() == 1)
            {
                DetailedJob.Start(args[0]);
            }
            else if (args.Count() > 1)
            {
                InstanceJob.Start(args[0], args[1]);
            }

            Maintenance.CleanupLogs();
        }

        static void InitializeLog()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            GlobalContext.Properties["AppData"] = appData;
            GlobalContext.Properties["LogName"] = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ssZ");
            XmlConfigurator.Configure(Assembly.GetManifestResourceStream("Ditto.Logging.Default.xml"));
        }

        static void AbortOnError(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
