using System;

namespace AutoRender {
    public class ProjectOverviewViewModel : BaseViewModel {
        //fields
        private string _strInfo = "";
        private bool _blnEditing = false;
        public static event EventHandler ProcessChanged;

        #region properties
        public MeltJob Job { get; private set; }

        public ProjectOverviewViewModel(MeltJob pJob) {
            Job = pJob;
            Job.StatusChanged += ObjJob_statusChanged;
            Job.ProgressChanged += ObjJob_progressChanged;
        }

        public string Name {
            get { return Job.Project.Name; }
        }

        public string TargetName {
            get { return Job.Project.TargetName; }
            set {
                Job.Project.TargetName = value;
                OnPropertyChanged("TargetName");
            }
        }

        public bool TargetExists {
            get { return Job.Project.TargetExists && (Job != null && Job.Status != JobStatus.Running); }
        }

        public bool Process {
            get { return Job.Project.Process; }
            set {
                Job.Project.Process = value;
                OnPropertyChanged("Process");

                ProcessChanged?.Invoke(this, new EventArgs());
            }
        }

        public JobStatus Status {
            get {
                return (Job == null) ? JobStatus.UnScheduled : Job.Status;
            }
        }

        public int Percentage {
            get {
                return (Job == null) ? 0 : Job.Percentage;
            }
        }

        public string Info {
            get {
                switch (Job.Status) {
                    case JobStatus.Exists:
                        return "Output file already exists";
                    case JobStatus.Failed:
                        return "Failed exporting file";
                    case JobStatus.Scheduled:
                        return "In Queue";
                    case JobStatus.Success:
                        return (Job.TimeSpent > 0) ? String.Format("Finished in {0}", TimeSpan.FromSeconds(Job.TimeSpent).ToString(@"hh\:mm\:ss")) : "";
                    case JobStatus.UnScheduled:
                    case JobStatus.Running:
                    default:
                        return "";
                }
            }
        }

        public bool AllowEditing {
            get {
                if (Editing) {
                    return false;
                }
                switch (Status) {
                    case JobStatus.Failed:
                    case JobStatus.Running:
                    case JobStatus.Scheduled:
                    case JobStatus.Success:
                        return false;
                    case JobStatus.UnScheduled:
                    default:
                        return true;
                }
            }
        }

        public bool Editing {
            get { return _blnEditing; }
            set {
                _blnEditing = value;
                OnPropertyChanged("Editing");
                OnPropertyChanged("AllowEditing");
                OnPropertyChanged("TargetExists");
                OnPropertyChanged("Process");
            }
        }
        #endregion

        #region Methods
        private void ObjJob_progressChanged(object sender, EventArgs e) {
            OnPropertyChanged("Percentage");
        }

        private void ObjJob_statusChanged(object sender, EventArgs e) {
            OnPropertyChanged("Status");
            OnPropertyChanged("AllowEditing");
            OnPropertyChanged("Info");
        }
        #endregion
    }
}