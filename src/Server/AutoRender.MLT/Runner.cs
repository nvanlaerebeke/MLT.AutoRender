using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AutoRender.Data;
using log4net;

namespace AutoRender.MLT {

    public enum ProcessStatus {
        Stopped,
        Paused,
        Running,
        Failed,
        Done
    }

    public class Runner {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<string> StdOut;

        public event EventHandler<ProcessStatus> StatusChanged;

        private readonly string Command;
        private readonly string Parameters;

        private Process _objProcess;
        private ProcessStatus _objStatus = ProcessStatus.Stopped;

        public ProcessStatus Status {
            get {
                return _objStatus;
            }
            private set {
                if (_objStatus != value) {
                    _objStatus = value;
                    StatusChanged?.Invoke(this, value);
                }
            }
        }

        public double TimeTaken { get; private set; } = 0;

        public Runner(string pCommand, string pParameters) {
            Command = pCommand;
            Parameters = pParameters;
            _objProcess = new Process();
        }

        public void Start() {
            if (Status == ProcessStatus.Paused) {
                Kill("CONT");
                Status = ProcessStatus.Running;
            } else {
                StartProcess();
            }
        }

        public void Stop() {
            if (_objProcess != null && !_objProcess.HasExited) {
                try {
                    _objProcess.Kill();
                } catch { }
                Status = ProcessStatus.Stopped;
            }
        }

        public void Pause() {
            Kill("STOP");
            Status = ProcessStatus.Paused;
        }

        private void StartProcess() {
            Status = ProcessStatus.Running;
            try {
                /*var strBasePath = Path.GetDirectoryName(Settings.MeltPath);
                var strModulePath = Path.Combine(strBasePath, "modules");
                var strProfilePath = Path.Combine(strBasePath, "profiles");
                var strPresetPath = Path.Combine(strBasePath, "presets");
                var strLibPath = $"{Path.Combine(strBasePath, "framework")}:{Path.Combine(strBasePath, "mlt++")}:{Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}";
                */
                /*Environment.SetEnvironmentVariable("MLT_REPOSITORY", strModulePath);
                Environment.SetEnvironmentVariable("MLT_DATA", strModulePath);
                Environment.SetEnvironmentVariable("MLT_PROFILES_PATH", strProfilePath);
                Environment.SetEnvironmentVariable("MLT_PRESETS_PATH", strPresetPath);
                Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", strLibPath);*/

                Log.Debug($"Running {Command} {Parameters}");

                _objProcess.StartInfo = new ProcessStartInfo(Command, Parameters) {
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };
                _objProcess.EnableRaisingEvents = true;
                _objProcess.OutputDataReceived += DataReceived;
                _objProcess.ErrorDataReceived += DataReceived_Error;
                _objProcess.Exited += _objProcess_Exited;
                _objProcess.Start();

                try {
                    _objProcess.BeginErrorReadLine();
                } catch { }
                try {
                    _objProcess.BeginOutputReadLine();
                } catch { }
                /*try {
                    _objProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                } catch { }*/

                _objProcess.WaitForExit();
            } catch (Exception) {
                Status = ProcessStatus.Failed;
            }
        }

        private void Kill(string pAction) {
            if (_objProcess != null && !_objProcess.HasExited) {
                try {
                    new Process() {
                        StartInfo = new ProcessStartInfo("kill", "-" + pAction.ToUpper() + " " + _objProcess.Id) {
                            UseShellExecute = false,
                            ErrorDialog = false,
                            CreateNoWindow = true,
                        }
                    }.Start();
                } catch (Exception) { }
            }
        }

        private void DataReceived(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                StdOut?.Invoke(this, e.Data.Trim());
            }
        }

        private void DataReceived_Error(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                StdOut?.Invoke(this, e.Data.Trim());
            }
        }

        private void _objProcess_Exited(object sender, System.EventArgs e) {
            _objProcess.OutputDataReceived -= DataReceived;
            _objProcess.ErrorDataReceived -= DataReceived;
            _objProcess.Exited -= _objProcess_Exited;

            Status = (_objProcess.ExitCode != 0) ? ProcessStatus.Failed : ProcessStatus.Done;
            TimeTaken = _objProcess.ExitTime.Subtract(_objProcess.StartTime).TotalSeconds;
        }
    }
}