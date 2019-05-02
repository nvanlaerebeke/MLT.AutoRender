using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace AutoRender.MLT {

    public class MeltJob {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MeltRunner MeltRunner;

        internal event EventHandler ProgressChanged;
        internal event EventHandler<JobStatus> StatusChanged;

        internal MLTProject Project { get; private set; }

        internal double TimeTaken {
            get {
                return MeltRunner.TimeTaken;
            }
        }

        internal long StartTime { get; private set; }

        //Properties
        private int _intPercentage = 0;

        internal int Percentage {
            get { return _intPercentage; }
            private set {
                if (value != _intPercentage) {
                    _intPercentage = value;
                    ProgressChanged?.Invoke(this, new System.EventArgs());
                }
            }
        }

        internal JobStatus Status {
            get {
                return MeltRunner.Status;
            }
        }

        internal MeltJob(MLTProject pProject) {
            Project = pProject;
            MeltRunner = new MeltRunner(Project.Config);
        }

        internal void Start() {
            if(Status == JobStatus.Running) { return; } // -- already running, ignore

            if (Status == JobStatus.Paused) {
                MeltRunner.Start();
            } else {

                StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                MeltRunner.ProgressChanged += ObjRunner_progressChanged;
                MeltRunner.StatusChanged += MeltRunner_StatusChanged;
                MeltRunner.Start();
            }
        }

        internal void Stop() {
            MeltRunner.Stop();
        }

        internal void Pause() {
            MeltRunner.Pause();
        }

        internal void Scheduled() {
            MeltRunner.Scheduled();
        }

        void MeltRunner_StatusChanged(object sender, JobStatus e) {
            switch(e) {
                case JobStatus.Failed:
                case JobStatus.Success:
                    MeltRunner.StatusChanged -= MeltRunner_StatusChanged;
                    MeltRunner.ProgressChanged -= ObjRunner_progressChanged;
                    Percentage = 0;
                    StartTime = 0;
                    break;
                case JobStatus.Paused:
                case JobStatus.Running:
                case JobStatus.Scheduled:
                case JobStatus.UnScheduled:
                    break;
            }
            StatusChanged?.Invoke(this, e);
        }


        private void ObjRunner_progressChanged(object sender, System.EventArgs e) {
            Log.Info("Job progress updated, forwarding as statusupdated");
            Percentage = (e as EventArgs.ProgressUpdatedEventArgs).Percentage;

            Task.Run(() => {
                StatusChanged?.Invoke(this, Status);
                if(StatusChanged == null) {
                    Log.Error("No one listening.....");
                }
            });
        }

        public static bool Equals(MeltJob obj1, MeltJob obj2) {
            return obj1 == obj2 || obj1 != null && obj1.Equals(obj2); // compare reference/value(null)
        }

        public bool Equals(MeltJob pJob) {
            return
                pJob != null && 
                Percentage.Equals(pJob.Percentage) &&
                Status.Equals(pJob.Status) &&
                TimeTaken.Equals(pJob.TimeTaken);
        }

        public override bool Equals(object obj) {
            return Equals(obj as MeltJob);
        }
    }
}