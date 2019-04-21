using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Collections.Generic;
using System.IO;

namespace AutoRender.Lib{
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
        public static ILog GetLogger(string pName) {
            ConfigureLogger(pName);
            return log4net.LogManager.GetLogger(pName);
        }

        private static void ConfigureLogger(string pName) {
            var LogFile = Path.Combine(Settings.LogDirectory, pName + ".log");
            var objLog = (log4net.Repository.Hierarchy.Logger)LogManager.GetLogger(pName).Logger;

            objLog.RemoveAllAppenders();
            objLog.Additivity = false;
            objLog.Level = log4net.Core.Level.All;

            CreateAppender(pName, LogFile).ForEach(objLog.AddAppender);

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Additivity = false;
            hierarchy.Configured = true;
        }

        private static List<IAppender> CreateAppender(string appenderName, string logFilename) {
            var objLayout = new PatternLayout("%date{dd/MM/yyyy HH:mm:ss} %message%newline") {
                IgnoresException = true
            };

            RollingFileAppender objFileAppender = new RollingFileAppender {
                AppendToFile = true,
                ImmediateFlush = true,
                Name = appenderName + "_file",
                Layout = objLayout,
                File = logFilename,
                MaxSizeRollBackups = 10,
            };
            objFileAppender.ActivateOptions();

            ConsoleAppender objConsoleAppender = new ConsoleAppender { 
                Layout = objLayout, 
                Name = appenderName + "_console", 
                Threshold = log4net.Core.Level.All 
            };
            objConsoleAppender.ActivateOptions();

            BasicConfigurator.Configure(objFileAppender);
            BasicConfigurator.Configure(objConsoleAppender);

            return new List<IAppender> { objFileAppender, objConsoleAppender };
        }
    }
}