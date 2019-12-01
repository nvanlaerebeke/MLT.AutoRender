using AutoRender.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoRender.MLT {

    public class MeltRunner {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MeltConfig Config;
        private readonly List<StdHandlers.StdHandler> Handlers;
        private JobStatus _objStatus = JobStatus.UnScheduled;
        private Runner Process;

        public event EventHandler ProgressChanged;

        public event EventHandler<JobStatus> StatusChanged;

        public JobStatus Status {
            get { return _objStatus; }
            private set {
                if (value != _objStatus) {
                    _objStatus = value;
                    StatusChanged?.Invoke(this, _objStatus);
                }
            }
        }

        public double TimeTaken {
            get {
                return (Process == null) ? 0 : Process.TimeTaken;
            }
        }

        internal MeltRunner(MeltConfig pConfig) {
            Config = pConfig;
            Handlers = MeltHelper.GetHandlers();
        }

        internal void Scheduled() {
            if (
                Status != JobStatus.Paused &&
                Status != JobStatus.Running
            ) {
                Status = JobStatus.Scheduled;
            }
        }

        public void Start() {
            _ = Task.Run(() => {
                if (Status == JobStatus.Paused && Process != null) {
                    Process.Start();
                    return;
                }
                Status = JobStatus.Running;

                if (File.Exists(Config.TempSourcePath)) { File.Delete(Config.TempSourcePath); }
                if (File.Exists(Config.TempTargetPath)) { File.Delete(Config.TempTargetPath); }
                Config.WriteConfig(); // -- ToDo: Pass target path for the config so that the file copy isn't needed
                File.Copy(Config.SourceFile, Config.TempSourcePath);

                /*if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    var strBasePath = Path.GetDirectoryName(Settings.MeltPath);
                    var strModulePath = Path.Combine(strBasePath, "modules");
                    var strProfilePath = Path.Combine(strBasePath, "profiles");
                    var strPresetPath = Path.Combine(strBasePath, "presets");
                    var strLibPath = $"{Path.Combine(strBasePath, "framework")}:{Path.Combine(strBasePath, "mlt++")}:{Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}";

                    Environment.SetEnvironmentVariable("MLT_REPOSITORY", strModulePath);
                    Environment.SetEnvironmentVariable("MLT_DATA", strModulePath);
                    Environment.SetEnvironmentVariable("MLT_PROFILES_PATH", strProfilePath);
                    Environment.SetEnvironmentVariable("MLT_PRESETS_PATH", strPresetPath);
                    Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", strLibPath);
                }*/
                var config = Regex.Replace(Config.ConfigFile, @"(\\+)$", @"$1$1");
                Process = new Runner(Settings.MeltPath, "-progress " + '"' + config + '"');
                Process.StatusChanged += _objProcess_StatusChanged;
                Process.StdOut += Process_StdOut;

                var objProgress = Handlers.First(h => h.GetType() == typeof(StdHandlers.Progress)) as StdHandlers.Progress;
                objProgress.ProgressUpdated += ObjProgress_ProgressUpdated;
                Process.Start();
            });
        }

        public void Stop() {
            Process.Stop();
        }

        public void Pause() {
            if (Status == JobStatus.Running) {
                Process.Pause();
            }
        }

        /// <summary>
        /// ToDo: is it needed to have 2 enums?
        /// </summary>
        /// <param name="pStatus">P status.</param>
        private void _objProcess_StatusChanged(object sender, ProcessStatus pStatus) {
            switch (pStatus) {
                case ProcessStatus.Done:
                    Finish();
                    Status = JobStatus.Success;
                    Cleanup();
                    break;

                case ProcessStatus.Failed:
                    Status = JobStatus.Failed;
                    Cleanup();
                    break;

                case ProcessStatus.Paused:
                    Status = JobStatus.Paused;
                    break;

                case ProcessStatus.Running:
                    Status = JobStatus.Running;
                    break;

                case ProcessStatus.Stopped:
                    Status = JobStatus.UnScheduled;
                    Cleanup();
                    break;
            }
        }

        private void Process_StdOut(object sender, string e) {
            if (!String.IsNullOrEmpty(e)) {
                Log.Info(e.Trim());
                Handlers.ForEach(h => h.Handle(e));
            }
        }

        private void Finish() {
            if (File.Exists(Config.TempTargetPath)) {
                var strNewName = Config.TargetPath;
                var i = 1;
                while (File.Exists(strNewName)) {
                    strNewName = Config.TargetPath.Replace("." + Path.GetExtension(strNewName), "") + "_" + i + Path.GetExtension(strNewName);
                    i++;
                };
                if (!new FileInfo(strNewName).Directory.Exists) {
                    Directory.CreateDirectory(new FileInfo(strNewName).Directory.FullName);
                }
                File.Move(Config.TempTargetPath, strNewName);
            }
        }

        private void ObjProgress_ProgressUpdated(object sender, System.EventArgs e) {
            Log.Info("Progress was changed");
            ProgressChanged?.Invoke(sender, e);
        }

        private void Cleanup() {
            Process.StatusChanged -= _objProcess_StatusChanged;
            Process.StdOut -= Process_StdOut;

            var objProgress = Handlers.First(h => h.GetType() == typeof(StdHandlers.Progress)) as StdHandlers.Progress;
            objProgress.ProgressUpdated -= ObjProgress_ProgressUpdated;

            // -- clean up up env
            if (File.Exists(Config.TempSourcePath)) { File.Delete(Config.TempSourcePath); }
            if (File.Exists(Config.TempTargetPath)) { File.Delete(Config.TempTargetPath); }

            var ts = new TimeSpan(0, 0, (int)TimeTaken);
            Log.Info(String.Format("Encoding completed after {0} Hours, {1} Minutes and {2} Seconds", ts.Hours, ts.Minutes, ts.Seconds));
        }
    }
}