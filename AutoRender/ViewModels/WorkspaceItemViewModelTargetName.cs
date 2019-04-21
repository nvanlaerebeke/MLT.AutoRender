namespace AutoRender {
    public partial class WorkspaceItemViewModel {
        public string TargetName {
            get {
                if (WorkspaceItem.Project != null) {
                    return WorkspaceItem.Project.TargetName;
                } else if(WorkspaceItem.Final != null) {
                    return WorkspaceItem.Final.Name;
                }
                return "";
            }
            set {
                if (WorkspaceItem.Project != null) {
                    WorkspaceItem.Project.TargetName = value;
                    OnPropertyChanged("TargetName");
                }
            }
        }
        public bool TargetNameAllowEditing {
            get {
                switch (Status) {
                    case Status.TargetExists:
                    case Status.Processable:
                    case Status.TargetInvalid:
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private bool _blnTargetNameIsEditing = false;
        public bool TargetNameIsEditing {
            get { return _blnTargetNameIsEditing; }
            set {
                _blnTargetNameIsEditing = value;
                OnPropertyChanged("TargetNameIsEditing");
                OnPropertyChanged("CanEditTargetName");
            }
        }
    }
}