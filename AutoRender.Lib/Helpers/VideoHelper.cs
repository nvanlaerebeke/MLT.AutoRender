using AutoRender.Lib;
using MediaToolkit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.Lib.Helpers {
    public class VideoHelper : CrazyUtils.Base {
        private Process _objProcess;
        private Thread _thdStdOut;
        private Thread _thdStdErr;
        private string _strPath;
        private bool _blnNoStream = false;

        private ManualResetEvent _objReadBlocker = new ManualResetEvent(false);

        public VideoHelper(string pPath) {
            _strPath = pPath;
        }

        public bool Valid() {
            Task.Run(() => {
                _objProcess = new Process();
                string strParams = "-i \"" + _strPath +"\"";
                ProcessStartInfo objStartInfo = new ProcessStartInfo(Settings.FfmpegPath, strParams);
                objStartInfo.UseShellExecute = false;
                objStartInfo.ErrorDialog = false;
                objStartInfo.CreateNoWindow = true;
                objStartInfo.StandardOutputEncoding = Encoding.UTF8;
                objStartInfo.StandardErrorEncoding = Encoding.UTF8;
                objStartInfo.RedirectStandardOutput = true;
                objStartInfo.RedirectStandardError = true;
                objStartInfo.WorkingDirectory = Settings.TempDirectory;

                _objProcess.EnableRaisingEvents = true;
                _objProcess.Exited += _objProcess_Exited;
                _objProcess.StartInfo = objStartInfo;

                int intIdentifier = new Random().Next(1, 9999);

                _thdStdErr = new Thread(readStdError);
                _thdStdErr.Name = "CMD_ERR-" + intIdentifier;
                _thdStdErr.IsBackground = true;

                _thdStdOut = new Thread(readStdOut);
                _thdStdOut.Name = "CMD_OUT-" + intIdentifier;
                _thdStdOut.IsBackground = true;

                try {
                    _objProcess.Start();
                    _thdStdOut.Start();
                    _thdStdErr.Start();
                } catch (Exception ex) {
                    Log.Error(ex);
                }
            });
            _objReadBlocker.WaitOne();
            return !_blnNoStream;
        }

        private void readStdOut() {
            StreamReader srStdOut = null;

            try {
                srStdOut = _objProcess.StandardOutput;
                string strLine = String.Empty;

                strLine = srStdOut.ReadLine();
                if (!String.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (_objProcess != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                    }
                    strLine = _objProcess.StandardOutput.ReadLine();
                }
            } catch (Exception ex) {
                Log.Error(ex);
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
                if (!String.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (_objProcess != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                    }
                    strLine = srStdErr.ReadLine();
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                if (srStdErr != null) {
                    srStdErr.Close();
                    srStdErr.Dispose();
                }
            }
        }

        private void _objProcess_Exited(object sender, System.EventArgs e) {
            int intExitCode = 4;
            try {
                if (_objProcess != null) {
                    // sometimes, _objProcess is null for unknown reasons
                    _objProcess.Exited -= _objProcess_Exited;
                }

                while ((_thdStdErr != null && _thdStdErr.IsAlive) || (_thdStdOut != null && _thdStdOut.IsAlive)) {
                    if (System.Threading.Thread.CurrentThread.Equals(_thdStdErr) || System.Threading.Thread.CurrentThread.Equals(_thdStdOut)) {
                        // this should never happen!!
                        break;
                    } else {
                        Thread.Sleep(50);
                    }
                }

                if (_objProcess != null) {
                    // sometimes, _objProcess is null for unknown reasons (suspended computer? crashed explorer?)                  
                    intExitCode = _objProcess.ExitCode;

                    if (intExitCode == 1) {
                        //Happens when process is killed
                        intExitCode = 3;
                    }
                }
                _objProcess = null;
            } catch (Exception ex) {
                Log.Error(ex);
            }
            _objReadBlocker.Set();
        }

        private void HandleLine(string pLine) {
            if (!String.IsNullOrEmpty(pLine)) {
                Console.WriteLine(pLine);
                if(pLine.Contains("Invalid data found when processing input")) {
                    _blnNoStream = true;
                }
            }
        }
    }
}