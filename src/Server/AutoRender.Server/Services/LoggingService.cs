using System;
using System.Diagnostics;
using System.IO;
using AutoRender.Logging;

namespace AutoRender.Server.Services {

    internal class LoggingService : Service {

        public LoggingService() {
        }

        public override void Start() {
            try {
                string LogDirectory;
                if (Debugger.IsAttached) {
                    LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoRender");
                } else {
                    LogDirectory = @"C:\AutoRender";
                }
                var LogFile = Path.Combine(LogDirectory, "AutoRender.log");
                if (!Directory.Exists(LogDirectory)) {
                    _ = Directory.CreateDirectory(LogDirectory);
                } else if (File.Exists(LogFile)) {
                    File.Delete(LogFile);
                }
                Logger.init(
                    log4net.Core.Level.Debug,
                    LogFile
                );
            } catch (Exception ex) {
                Log.Error("Failed to set up the logging, continue with default");
                Log.Error(ex);
                Console.WriteLine(ex);
            }
        }

        public override void Stop() {
            //does nothing
        }

        /// <summary>
        /// Logging has highest priority - that way if another service logs anything, it can be seen
        /// </summary>
        public override byte Priority {
            get { return byte.MaxValue; }
        }
    }
}