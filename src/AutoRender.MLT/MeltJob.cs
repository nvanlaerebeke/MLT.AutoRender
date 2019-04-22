using System;

namespace AutoRender.MLT {

    internal class MeltJob {
        private MeltRunner _objMeltProcess;

        //Events
        internal event EventHandler ProgressChanged;

        internal event EventHandler StatusChanged;

        //fields
        private JobStatus _objStatus = JobStatus.UnScheduled;

        internal MLTProject Project { get; private set; }

        internal double TimeTaken {
            get {
                return (_objMeltProcess != null) ? _objMeltProcess.TimeTaken : 0;
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
                return _objStatus;
            }
            private set {
                if (value != _objStatus) {
                    _objStatus = value;
                    StatusChanged?.Invoke(this, new EventArgs.StatusChangedEventArgs(_objStatus));
                }
            }
        }

        internal MeltJob(MLTProject pProject) {
            Project = pProject;
        }

        internal void Schedule() {
            Status = JobStatus.Scheduled;
            MeltJobScheduler.Schedule(this);
        }

        internal void Start() {
            if (_objMeltProcess != null) { Stop(); }

            StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _objMeltProcess = new MeltRunner(Project.Config);
            _objMeltProcess.StatusChanged += ObjRunner_complete;
            _objMeltProcess.ProgressChanged += ObjRunner_progressChanged;
            _objMeltProcess.Start();

            Status = JobStatus.Running;
        }

        internal void Stop() {
            if (_objMeltProcess != null) {
                StartTime = 0;
                _objMeltProcess.StatusChanged -= ObjRunner_complete;
                _objMeltProcess.ProgressChanged -= ObjRunner_progressChanged;
                _objMeltProcess.Stop();
                _objMeltProcess = null;

                Status = JobStatus.UnScheduled;
            }
        }

        internal void Pause() {
            if (_objMeltProcess != null && _objMeltProcess.Status == JobStatus.Running) {
                _objMeltProcess.Pause();
            }
        }

        internal void Resume() {
            if (_objMeltProcess != null && _objMeltProcess.Status == JobStatus.Paused) {
                _objMeltProcess.Start();
            }
        }

        private void ObjRunner_progressChanged(object sender, System.EventArgs e) {
            Percentage = (e as EventArgs.ProgressUpdatedEventArgs).Percentage;
        }

        private void ObjRunner_complete(object sender, System.EventArgs e) {
            if (
                _objMeltProcess.Status != JobStatus.Running &&
                _objMeltProcess.Status != JobStatus.Paused
            ) {
                Percentage = 0;
                StartTime = 0;
            }
            this.Status = _objMeltProcess.Status;
        }

        public static bool Equals(MeltJob obj1, MeltJob obj2) {
            return obj1 == obj2 || obj1 != null && obj1.Equals(obj2); // compare reference/value(null)
        }

        public bool Equals(MeltJob pJob) {
            return
                pJob != null
                && this.Percentage.Equals(pJob.Percentage) &&
                this.Status.Equals(pJob.Status) &&
                this.TimeTaken.Equals(pJob.TimeTaken);
        }

        public override bool Equals(object obj) {
            return this.Equals(obj as MeltJob);
        }
    }
}