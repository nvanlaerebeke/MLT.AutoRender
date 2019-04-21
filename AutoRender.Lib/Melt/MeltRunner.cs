using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoRender.Lib.Melt {
    public class MeltRunner : CrazyUtils.Base {
        private Process _objProcess;
        private Thread _thdStdOut;
        private Thread _thdStdErr;
        private JobStatus _objStatus;
        private MeltConfig Config;
        private ILog ProjectLog;
        private CancellationTokenSource _objJobCancelationSource = new CancellationTokenSource();
        private CancellationToken _objJobCancelationToken;

        public event EventHandler ProgressChanged;
        public event EventHandler StatusChanged;
        internal double TimeTaken { get; private set; }

        private List<StdHandlers.StdHandler> _lstHandlers;


        public JobStatus Status {
            get { return _objStatus; }
            private set {
                if (value != _objStatus) {
                    _objStatus = value;
                    StatusChanged?.Invoke(this, new System.EventArgs());
                }
            }
        }


        internal MeltRunner(MeltConfig pConfig) {
            Config = pConfig;
            ProjectLog = Logger.GetLogger(Path.GetFileName(pConfig.TargetPath));

            Status = JobStatus.UnScheduled;
            _lstHandlers = Helpers.MeltHelper.GetHandlers();

            _objJobCancelationToken = _objJobCancelationSource.Token;

            //attach events
            var objProgress = _lstHandlers.First(h => h.GetType() == typeof(StdHandlers.Progress)) as StdHandlers.Progress;
            objProgress.progressUpdated += delegate (object sender, System.EventArgs e) {
                ProgressChanged?.Invoke(sender, e);
            };
        }


        public void Start() {
            if (Status == JobStatus.Paused) {
                ResumeEncoding();
            } else {
                _objJobCancelationToken.Register(StopEncoding);
                StartEncoding();
            }
        }

        public void Stop() {
            StopEncoding();
        }

        public void Pause() {
            PauseEncoding();
        }

        private void StartEncoding() {
            Status = JobStatus.Running;

            var strCommand = "-progress " + "\"" + Regex.Replace(Config.ConfigFile, @"(\\+)$", @"$1$1") + "\"";
            _objProcess = new Process();

            ProcessStartInfo objStartInfo = new ProcessStartInfo(Settings.MeltPath, strCommand) {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Settings.TempDirectory
            };

            _objProcess.EnableRaisingEvents = true;
            _objProcess.Exited += _objProcess_Exited;
            _objProcess.StartInfo = objStartInfo;

            int intIdentifier = new Random().Next(1, 9999);

            _thdStdErr = new Thread(readStdError) {
                IsBackground = true
            };

            _thdStdOut = new Thread(readStdOut) {
                IsBackground = true
            };

            try {
                if (File.Exists(Config.TempSourcePath)) { File.Delete(Config.TempSourcePath); }
                if (File.Exists(Config.TempTargetPath)) { File.Delete(Config.TempTargetPath); }
                File.Copy(Config.SourceFile, Config.TempSourcePath);

                _objProcess.Start();
                _thdStdOut.Start();
                _thdStdErr.Start();
                try {
                    _objProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                } catch (Exception ex) {
                    ProjectLog.Error(ex);
                }
            } catch (Exception ex) {
                ProjectLog.Error(ex);
            }
        }

        private void StopEncoding() {
            _objJobCancelationSource.Cancel();
            if (_objProcess != null) {
                _objProcess.Kill();
                Status = JobStatus.UnScheduled;
            }
        }

        private void readStdOut() {
            StreamReader srStdOut = null;

            try {
                srStdOut = _objProcess.StandardOutput;
                string strLine = String.Empty;

                strLine = srStdOut.ReadLine();
                while ((strLine != null) && (_objProcess != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                        ProjectLog.Info("[" + DateTime.Now.ToShortTimeString() + "] " + strLine);
                    }
                    strLine = _objProcess.StandardOutput.ReadLine();
                }
            } catch (Exception ex) {
                ProjectLog.Error(ex);
            } finally {
                if (srStdOut != null) {
                    srStdOut.Close();
                    srStdOut.Dispose();
                }
            }
        }

        private void readStdError() {
            StreamReader srStdErr = null;
            try {
                srStdErr = _objProcess.StandardError;
                string strLine = String.Empty;

                strLine = srStdErr.ReadLine();
                while ((strLine != null) && (_objProcess != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                        ProjectLog.Info("[" + DateTime.Now.ToShortTimeString() + "] " + strLine);
                    }
                    strLine = srStdErr.ReadLine();
                }
            } catch (Exception ex) {
                ProjectLog.Error(ex);
            } finally {
                if (srStdErr != null) {
                    srStdErr.Close();
                    srStdErr.Dispose();
                }
            }
        }

        private void _objProcess_Exited(object sender, System.EventArgs e) {
            _objProcess.Exited -= _objProcess_Exited;

            // -- clean up up env
            if (File.Exists(Config.TempSourcePath)) { File.Delete(Config.TempSourcePath); }

            if (_objProcess.ExitCode != 0) { // -- when not success, remove the temp file
                if (File.Exists(Config.TempTargetPath)) { File.Delete(Config.TempTargetPath); }
            } else { // -- when success, move to final location
                if (File.Exists(Config.TempTargetPath)) {
                    var strNewName = Config.TargetPath;
                    var i = 1;
                    while (File.Exists(strNewName)) {
                        strNewName = Config.TargetPath.Replace("." + Path.GetExtension(strNewName), "") + "_" + i + Path.GetExtension(strNewName);
                        i++;
                    };
                    if(!new FileInfo(strNewName).Directory.Exists) {
                        Directory.CreateDirectory(new FileInfo(strNewName).Directory.FullName);
                    }
                    File.Move(Config.TempTargetPath, strNewName);
                    Status = JobStatus.Success;
                }
            }

            TimeSpan ts = _objProcess.ExitTime.Subtract(_objProcess.StartTime);
            TimeTaken = ts.TotalSeconds;
            ProjectLog.Info(String.Format("Encoding completed after {0} Hours, {1} Minutes and {2} Seconds", ts.Hours, ts.Minutes, ts.Seconds));

            if (Status == JobStatus.Running || Status == JobStatus.Paused) { Status = JobStatus.Failed; }
        }

        private void PauseEncoding() {
            Kill("STOP");
            Status = JobStatus.Paused;
        }

        private void ResumeEncoding() {
            Kill("CONT");
            Status = JobStatus.Running;
        }

        private void Kill(string pAction) {
            if (_objProcess != null && !_objProcess.HasExited) {
                var objKill = new Process() {
                    StartInfo = new ProcessStartInfo("kill", "-" + pAction.ToUpper() + " " + _objProcess.Id) {
                        UseShellExecute = false,
                        ErrorDialog = false,
                        CreateNoWindow = true,
                    }
                };
                try {
                    objKill.Start();
                } catch (Exception ex) {
                    ProjectLog.Error(ex);
                }
            }
        }

        private void HandleLine(string pLine) {
            if (!String.IsNullOrEmpty(pLine)) {
                _lstHandlers.ForEach(h => h.Handle(pLine));
            }
        }
    }
}