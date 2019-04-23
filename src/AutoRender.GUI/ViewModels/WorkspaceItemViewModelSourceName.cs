using System;

namespace AutoRender {
    public partial class WorkspaceItemViewModel {
        public event EventHandler SourceNameUpdated;
        public string SourceName {
            get {
                if (WorkspaceItem.Project != null) {
                    return WorkspaceItem.Project.SourceName;
                }
                if (WorkspaceItem.New != null) {
                    return WorkspaceItem.New.Name;
                }
                return "";
            }
            set {
                if (WorkspaceItem.Project != null) {
                    WorkspaceItem.Project.SourceName = value;
                    OnPropertyChanged("SourceName");
                    SourceNameUpdated?.Invoke(this, new EventArgs());
                }
            }
        }
        public bool SourceNameAllowEditing {
            get {
                switch (Status) {
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.Processable:
                        return true;
                    case Status.TargetExists:
                    case Status.TargetInvalid:
                    case Status.ProjectMissing:
                    case Status.Busy:
                    case Status.Queued:
                    case Status.Finished:
                    case Status.Unknown:
                    case Status.Error:
                    case Status.Updating:
                    default:
                        return false;
                }
            }
        }

        private bool _blnSourceNameIsEditing = false;
        public bool SourceNameIsEditing {
            get { return _blnSourceNameIsEditing; }
            set {
                _blnSourceNameIsEditing = value;
                OnPropertyChanged("SourceNameIsEditing");
                OnPropertyChanged("CanEditSourceName");
            }
        }
    }
}
