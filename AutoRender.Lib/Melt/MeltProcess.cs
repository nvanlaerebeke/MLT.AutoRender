using CrazyUtils;
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
    public class MeltProcess {
        private ProcessWrapper _objProcess;
        private Project _objProject;
        private JobStatus _objStatus;

        public event EventHandler ProgressChanged;
        public event EventHandler StatusChanged;

        private List<StdHandlers.StdHandler> _lstHandlers;

        public JobStatus Status {
            get { return _objStatus; }
            private set {
                _objStatus = value;
                StatusChanged?.Invoke(this, new System.EventArgs());
            }
        }

        public MeltProcess(Project pProject) {
            Status = JobStatus.UnScheduled;
            _objProject = pProject;
            _lstHandlers = Helpers.MeltHelper.GetHandlers();

            //attach events
            var objProgress = _lstHandlers.First(h => h.GetType() == typeof(StdHandlers.Progress)) as StdHandlers.Progress;
            objProgress.progressUpdated += delegate (object sender, System.EventArgs e) {
                ProgressChanged?.Invoke(sender, e);
            };

            _objProcess = new ProcessWrapper(Settings.MeltPath, "-progress " + "\"" + Regex.Replace(_objProject.ConfigFile, @"(\\+)$", @"$1$1") + "\"");
            _objProcess.Output += (pOutput) => {
                if (!String.IsNullOrEmpty(pOutput)) {
                    _lstHandlers.ForEach(h => h.Handle(pOutput));
                }
            };
            _objProcess.StatusChanged += (ProcessStatus pStatus) => {
                if(pStatus == ProcessStatus.Success) {
                    // -- clean up up env
                    if (File.Exists(_objProject._objConfig.TempSourcePath)) {
                        File.Delete(_objProject._objConfig.TempSourcePath);
                    }
                    if (File.Exists(_objProject._objConfig.TempTargetPath)) {
                        if (File.Exists(_objProject._objConfig.TargetPath)) {
                            //change name
                            var strNewName = "";
                            var i = 1;
                            do {
                                strNewName = _objProject._objConfig.TargetPath + "_" + i;
                                i++;
                            } while (File.Exists(strNewName));
                            File.Move(_objProject._objConfig.TempTargetPath, strNewName);
                        } else {
                            File.Move(_objProject._objConfig.TempTargetPath, _objProject._objConfig.TargetPath);
                        }
                    }
                }

                Status = (JobStatus)Enum.Parse(typeof(JobStatus), pStatus.ToString());
            };
        }


        public void Start() {
            if (_objProcess.Status == ProcessStatus.Paused) {
                _objProcess.Resume();
            } else {
                Status = JobStatus.Running;
                //Copy source file to temp location
                if (!File.Exists(_objProject._objConfig.TempSourcePath)) {
                    File.Copy(_objProject._objConfig.SourceFile, _objProject._objConfig.TempSourcePath);
                }
                _objProcess.Start();
            }
        }

        public void Stop() {
            _objProcess.Stop();
        }

        public void Pause() {
            _objProcess.Pause();
        }
    }
}