using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AutoRender.Lib.Melt {
    public static class MeltJobScheduler {
        private static bool _blnInitialized = false;
        private static BlockingCollection<MeltJob> _colJobs = new BlockingCollection<MeltJob>();
        private static List<MeltJob> _lstProcessing = new List<MeltJob>();
        private static ManualResetEvent _objMaxThreadBlocker = new ManualResetEvent(false);

        internal static List<MeltJob> GetAll() {
            var lstJobs = new List<MeltJob>(_colJobs);
            lstJobs.AddRange(_lstProcessing);
            return lstJobs;
        }

        internal static void Schedule(MeltJob pJob) {
            if (!_blnInitialized) { StartProcessing(); }
            _colJobs.Add(pJob);
        }

        private static void StartProcessing() {
            Thread objQueueThread = new Thread(() => {
                Thread.CurrentThread.Name = "MeltJobScheduler";
                while (true) {
                    var objJob = _colJobs.Take();
                    while (_lstProcessing.Count >= Settings.Threads) {
                        _objMaxThreadBlocker.WaitOne();
                        _objMaxThreadBlocker.Reset();
                    }
                    lock (_lstProcessing) {
                        _lstProcessing.Add(objJob);
                    }
                    Start(objJob);
                }
            }) {
                IsBackground = true
            };
            objQueueThread.Start();
        }

        private static void Start(MeltJob pJob) {
            Thread objEncoder = new Thread(() => {
                Thread.CurrentThread.Name = "MeltJobProcessor " + pJob.Project.Name;
                pJob.Start();
                pJob.StatusChanged += pJob_StatusChanged;
            }) {
                IsBackground = true
            };
            objEncoder.Start();
        }

        private static void pJob_StatusChanged(object sender, System.EventArgs e) {
            var objStatus = (e as EventArgs.StatusChangedEventArgs).Status;
            if (
                objStatus == JobStatus.Success ||
                objStatus == JobStatus.Failed
            ) {
                lock (_lstProcessing) {
                    _lstProcessing.Remove((MeltJob)sender);
                }
                _objMaxThreadBlocker.Set();
            }
        }
    }
}
