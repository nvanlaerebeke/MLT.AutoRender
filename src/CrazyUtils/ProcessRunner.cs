using System;
using System.Diagnostics;
using System.Text;

namespace CrazyUtils {
    public enum ProcessStatus { 
        Stopped,
        Paused,
        Running,
        Failed,
        Done
    }

    public class ProcessRunner {
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

        public ProcessRunner(string pCommand, string pParameters) {
            Command = pCommand;
            Parameters = pParameters;
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
                _objProcess = new Process() {
                    StartInfo = new ProcessStartInfo(Command, Parameters) {
                        UseShellExecute = false,
                        ErrorDialog = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true
                    },
                    EnableRaisingEvents = true
                };

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
                try {
                    _objProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                } catch { }

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

        void DataReceived(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                StdOut?.Invoke(this, e.Data.Trim());
            }
        }
        void DataReceived_Error(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                StdOut?.Invoke(this, e.Data.Trim());
            }
        }

        void _objProcess_Exited(object sender, EventArgs e) {
            _objProcess.OutputDataReceived -= DataReceived;
            _objProcess.ErrorDataReceived -= DataReceived;
            _objProcess.Exited -= _objProcess_Exited;

            Status = (_objProcess.ExitCode != 0) ? ProcessStatus.Failed : ProcessStatus.Done;
            TimeTaken = _objProcess.ExitTime.Subtract(_objProcess.StartTime).TotalSeconds;
        }
    }
}