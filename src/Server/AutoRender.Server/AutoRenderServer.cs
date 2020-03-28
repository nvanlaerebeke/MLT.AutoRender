using System;
using System.IO;
using System.Reflection;
using AutoRender.Data;
using log4net;

namespace AutoRender.Server {

    public class AutoRenderServer {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServiceManager ServiceManager;

        public AutoRenderServer() {
            ServiceManager = new ServiceManager();
        }

        public void Start() {
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                Environment.SetEnvironmentVariable("MONO_REGISTRY_PATH", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "registry"));
            }
            try {
                Cleanup();
            } catch (Exception ex) {
                Log.Error("Failed to run the cleanup");
                Log.Error(ex);
            }
            ServiceManager.Start();
        }

        public void Stop() {
            ServiceManager.Stop();
        }

        private void Cleanup() {
            try {
                Log.Debug("Cleaning up temp directory");
                if (Directory.Exists(Settings.TempDirectory)) {
                    Directory.Delete(Settings.TempDirectory, true);
                }
            } catch (Exception ex) {
                Log.Error($"Failed to clean up {Settings.TempDirectory}: {ex.Message}");
            }
            if (!Directory.Exists(Settings.TempDirectory)) {
                Directory.CreateDirectory(Settings.TempDirectory);
            }
        }
    }
}