using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;

namespace AutoRender.Logging {

    public static class Logger {
        private static object LOCK = new object();

        public static void init(Level pLogLevel, string pLogFile) {
            lock (LOCK) {
                //Configure log4net
                var patternLayout = new PatternLayout() {
                    ConversionPattern = "%date [%thread] %-5level %logger{1} - %message%newline"
                };
                patternLayout.ActivateOptions();

                var hierarchy = (Hierarchy)LogManager.GetRepository();
                hierarchy.Root.Level = pLogLevel;

                if (!String.IsNullOrEmpty(pLogFile)) {
                    var roller = new RollingFileAppender {
                        AppendToFile = true,
                        File = pLogFile,
                        Layout = patternLayout,
                        ImmediateFlush = true,
                        MaxSizeRollBackups = 5,
                        MaximumFileSize = "10MB",
                        RollingStyle = RollingFileAppender.RollingMode.Size,
                        StaticLogFileName = true
                    };
                    roller.ActivateOptions();
                    hierarchy.Root.AddAppender(roller);


#if DEBUG
                    var debugAppender = new ConsoleAppender
                    {
                        Layout = patternLayout
                    };
                    debugAppender.ActivateOptions();
                    hierarchy.Root.AddAppender(debugAppender);
#endif
                }
                else {
                    var consoleappender = new ConsoleAppender {
                        Layout = patternLayout
                    };
                    consoleappender.ActivateOptions();
                    hierarchy.Root.AddAppender(consoleappender);
                }


                hierarchy.Configured = true;
            }
        }
    }
}