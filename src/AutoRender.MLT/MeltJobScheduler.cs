using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.MLT {

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

        internal void Schedule(MeltJob pJob) {
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
            ManualResetEvent objWait = new ManualResetEvent(false);

            pJob.StatusChanged += (sender, e) => { 
                switch(e) {
                    case JobStatus.Failed:
                    case JobStatus.Success:
                        if(Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        objWait.Set();
                        break;
                    case JobStatus.Paused:
                        if (Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        Paused.TryAdd(pJob.Project.ID.ToString(), pJob);
                        objWait.Set();
                        break;
                    case JobStatus.UnScheduled:
                        if(Paused.ContainsKey(pJob.Project.ID.ToString())) {
                            Paused.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        if (Running.ContainsKey(pJob.Project.ID.ToString())) {
                            Running.TryRemove(pJob.Project.ID.ToString(), out _);
                        }
                        objWait.Set();
                        break;
                    case JobStatus.Running:
                    case JobStatus.Scheduled:
                        break;
                }
            };
            Task.Run(() => { 
                pJob.Start();
            });
            objWait.WaitOne();
        }
    }
}