using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace CrazyUtils {

    public delegate void Output(string pOutput);

    public delegate void ProcessStatusChanged(ProcessStatus pStatus);

    public class ProcessWrapper {
        private Process _objProcess;
        private Thread _thdStdOut;
        private Thread _thdStdErr;
        private ProcessStatus _objStatus = ProcessStatus.Pending;

        private CancellationTokenSource _objJobCancelationSource = new CancellationTokenSource();
        private CancellationToken _objJobCancelationToken;

        public event ProcessStatusChanged StatusChanged;

        public event Output Output;

        public ProcessStatus Status {
            get { return _objStatus; }
            private set {
                if (_objStatus != value) {
                    _objStatus = value;
                    StatusChanged?.Invoke(_objStatus);
                }
            }
        }

        private string _strExecutable;
        private string _strParams;

        public ProcessWrapper(string pExecutable, string pParams) {
            pExecutable = "sleep";
            pParams = "10";
            //var strCommand = "-progress " + "\"" + Regex.Replace("/mnt/nas/Video/TestInbox/Temp/test1.xml", @"(\\+)$", @"$1$1") + "\"";
            //pExecutable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "melt/bin/melt");
            _strExecutable = pExecutable;
            _strParams = pParams;

            Status = ProcessStatus.Pending;
        }

        public void Start() {
            _objProcess = new Process();
            ProcessStartInfo objStartInfo = new ProcessStartInfo(_strExecutable, _strParams) {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
            };
            _objProcess.EnableRaisingEvents = true;
            _objProcess.Exited += _objProcess_Exited;
            _objProcess.StartInfo = objStartInfo;

            int intIdentifier = new Random().Next(1, 9999);

            _thdStdErr = new Thread(readStdErr);
            _thdStdErr.Name = "CMD_ERR-" + intIdentifier;
            _thdStdErr.IsBackground = true;

            _thdStdOut = new Thread(readStdOut);
            _thdStdOut.Name = "CMD_OUT-" + intIdentifier;
            _thdStdOut.IsBackground = true;
            try {
                _objProcess.Start();
                _thdStdOut.Start();
                _thdStdErr.Start();

                Status = ProcessStatus.Running;
            } catch (Exception ex) {
                //Log.Error(ex);
                Status = ProcessStatus.Failed;
            }
        }

        public void Stop() {
            if (!_objProcess.HasExited) {
                _objProcess.Kill();
            }
        }

        public void Pause() {
            if (Status == ProcessStatus.Running && !_objProcess.HasExited) {
                DoKill("STOP");
                Status = ProcessStatus.Paused;
            }
        }

        public void Resume() {
            if (Status == ProcessStatus.Paused && !_objProcess.HasExited) {
                DoKill("CONT");
                Status = ProcessStatus.Running;
            }
        }

        private void readStdOut() {
            StreamReader srStdOut = null;

            try {
                srStdOut = _objProcess.StandardOutput;
                string strLine = String.Empty;
                do {
                    strLine = srStdOut.ReadLine();
                    Console.WriteLine(strLine);
                    if (!String.IsNullOrEmpty(strLine)) {
                        Output?.Invoke(strLine.Trim());
                    }
                } while (strLine != null && _objProcess != null && !_objProcess.HasExited);
            } catch (Exception ex) {
                //Log.Error(ex);
            }
        }

        private void readStdErr() {
            StreamReader srStdOut = null;

            try {
                srStdOut = _objProcess.StandardOutput;
                string strLine = String.Empty;
                do {
                    strLine = srStdOut.ReadLine();
                    Console.WriteLine(strLine);
                    if (!String.IsNullOrEmpty(strLine)) {
                        Output?.Invoke(strLine.Trim());
                    }
                } while (strLine != null && _objProcess != null && !_objProcess.HasExited);
            } catch (Exception ex) {
                //Log.Error(ex);
            }
        }

        private void _objProcess_Exited(object sender, EventArgs e) {
            Status = (_objProcess.ExitCode == 0) ? ProcessStatus.Success : ProcessStatus.Failed;
        }

        private void DoKill(string pCommand) {
            if (_objProcess != null && !_objProcess.HasExited) {
                var objPause = new Process();
                ProcessStartInfo objStartInfo = new ProcessStartInfo("kill", " -" + pCommand + " " + _objProcess.Id) {
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                };

                try {
                    objPause.Start();
                } catch (Exception ex) {
                    //Log.Error(ex);
                }
            }
        }
    }

    public enum ProcessStatus {
        Pending,
        Running,
        Success,
        Failed,
        Paused
    }
}