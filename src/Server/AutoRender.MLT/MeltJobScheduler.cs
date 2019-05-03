using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.MLT {
    /*public static class MeltJobScheduler {
        private static bool _blnInitialized = false;
        private static BlockingCollection<MeltJob> _colJobs = new BlockingCollection<MeltJob>();
        private static List<MeltJob> _lstProcessing = new List<MeltJob>();
        private static ManualResetEvent _objMaxThreadBlocker = new ManualResetEvent(false);

        internal static List<MeltJob> GetAll() {
            var lstJobs = new List<MeltJob>(_colJobs);
            lstJobs.AddRange(_lstProcessing);
            return lstJobs;
        }

        public static void Schedule(MeltJob pJob) {
            if (!_blnInitialized) {
                _blnInitialized = true;
                StartProcessing(); 
            }
            pJob.Scheduled();
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

        private static void pJob_StatusChanged(object sender, JobStatus e) {
            var pJob = sender as MeltJob;
            switch (e) {
                case JobStatus.Failed:
                case JobStatus.Success:
                    if (_lstProcessing.Contains(pJob)) {
                        _lstProcessing.Remove(pJob);
                    }
                    _objMaxThreadBlocker.Set();
                    break;
                case JobStatus.Paused:
                 
                    //_objMaxThreadBlocker.Set();
                    break;
                case JobStatus.UnScheduled:

                    //_objMaxThreadBlocker.Set();
                    break;
                case JobStatus.Running:
                case JobStatus.Scheduled:
                    break;
            }
                    
        }
    }*/
    public class MeltJobScheduler {
        private static readonly MeltJobScheduler Scheduler = new MeltJobScheduler(2);

        public static MeltJobScheduler GetScheduler() {
            return Scheduler;
        }

        private readonly BlockingCollection<MeltJob> Queue = new BlockingCollection<MeltJob>();
        private readonly ConcurrentDictionary<string, MeltJob> Running = new ConcurrentDictionary<string, MeltJob>();
        private readonly ConcurrentDictionary<string, MeltJob> Paused = new ConcurrentDictionary<string, MeltJob>();

        public MeltJobScheduler(int pThreads) {
            Start(pThreads);
        }

        public void Schedule(MeltJob pJob) {
            if (Paused.ContainsKey(pJob.Project.ID.ToString())) {
                if (Paused.TryRemove(pJob.Project.ID.ToString(), out MeltJob objJob)) {
                    objJob.Scheduled();
                    Queue.Add(objJob);
                }
            } else {
                pJob.Scheduled();
                Queue.Add(pJob);
            }
        }

        internal bool IsRunning(MLTProject pProject) {
            return Running.ContainsKey(pProject.ID.ToString());
        }

        private void Start(int pThreads) {
            Task.Run(() => {
                Parallel.For(0, pThreads, (i) => {
                    while (true) {
                        MeltJob objJob = Queue.Take();
                        if (
                            objJob.Status == JobStatus.Scheduled ||
                            objJob.Status == JobStatus.Paused
                        ) {
                            if (Running.TryAdd(objJob.Project.ID.ToString(), objJob)) {
                                StartJob(objJob);
                            } else {
                                Queue.Add(objJob);
                            }
                        }
                    }
                });
            });
        }

        private void StartJob(MeltJob pJob) {
            pJob.StatusChanged += (sender, e) => {
                switch (e) {
                    case JobStatus.Failed:
                    case JobStatus.Success:
                        if (Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        break;
                    case JobStatus.Paused:
                        if (Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        Paused.TryAdd(pJob.Project.ID.ToString(), pJob);
                        break;
                    case JobStatus.UnScheduled:
                        if (Paused.ContainsKey(pJob.Project.ID.ToString())) {
                            Paused.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        if (Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        break;
                    case JobStatus.Running:
                    case JobStatus.Scheduled:
                        break;
                }
            };
            pJob.Start().Wait();
        }
    }
}