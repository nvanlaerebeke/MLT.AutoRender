using log4net;
using log4net.Layout;
using System;
using System.IO;
using System.Reflection;

namespace AutoRender.Logging {

    public class LogProvider : Mitto.ILogging.ILogProvider {

        public LogProvider() {
            //Configure log4net
            var patternLayout = new PatternLayout() {
                ConversionPattern = "%date [%thread] %-5level %logger{1} - %message%newline"
            };
            patternLayout.ActivateOptions();

            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            hierarchy.Root.Level = log4net.Core.Level.Debug;

            var strLogFile = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Logs",
                Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".log"
            );

            var roller = new log4net.Appender.RollingFileAppender {
                AppendToFile = true,
                File = strLogFile,
                Layout = patternLayout,
                ImmediateFlush = true,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "10MB",
                RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);
#if DEBUG
            var consoleappender = new log4net.Appender.ConsoleAppender {
                Layout = patternLayout
            };
            consoleappender.ActivateOptions();
            hierarchy.Root.AddAppender(consoleappender);
#endif
            hierarchy.Configured = true;
        }

        public Mitto.ILogging.ILog GetLogger(Type pType) {
            return new Log(LogManager.GetLogger(pType));
        }

        public Mitto.ILogging.ILog GetLogger() {
            return new Log(LogManager.GetLogger("Main"));
        }
    }
}