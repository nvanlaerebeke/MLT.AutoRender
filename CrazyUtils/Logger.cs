using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.IO;

namespace CrazyUtils{
    public static  class Logger {
        public static void init() {
            ConsoleAppender consAppender = new ConsoleAppender();
            consAppender.Threshold = log4net.Core.Level.All;
            consAppender.Name = "ConsoleAppender";
            consAppender.Layout = new log4net.Layout.SimpleLayout();

            //var appender = CreateAppender("Main", Path.Combine(Settings.LogDirectory, "AutoRender.log"));
            //BasicConfigurator.Configure(appender);
            BasicConfigurator.Configure(consAppender);
        }

        public static ILog GetLogger() {
            ConfigureLogger("AutoRender");
            return log4net.LogManager.GetLogger("AutoRender");
        }


        private static void ConfigureLogger(string pProjectName) {
            string LogFile = Path.Combine(Directory.GetCurrentDirectory(), pProjectName + ".log");
            string appenderName = string.Format("MeltLog_{0}", pProjectName);
            var syncLogAppender = CreateAppender(appenderName, LogFile);
            var syncLog = (log4net.Repository.Hierarchy.Logger)LogManager.GetLogger(appenderName).Logger;

            syncLog.RemoveAllAppenders();
            syncLog.Additivity = false;
            syncLog.AddAppender(syncLogAppender);
            syncLog.Level = log4net.Core.Level.All;

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Additivity = false;
            hierarchy.Configured = true;
        }

        private static FileAppender CreateAppender(string appenderName, string logFilename) {
            var objLayout = new PatternLayout("%date{dd/MM/yyyy HH:mm:ss} %message%newline");
            objLayout.IgnoresException = true;

            RollingFileAppender result;
            result = new RollingFileAppender {
                AppendToFile = true,
                ImmediateFlush = true,
                Name = appenderName,
                Layout = objLayout,
                File = logFilename,
                MaxSizeRollBackups = 10
            };
            result.ActivateOptions();
            return result;
        }
    }
}