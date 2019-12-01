using System;
using AutoRender.Data;
using AutoRender.Messaging;

namespace AutoRender {

    public partial class WorkspaceItemViewModel {

        public bool Exists {
            get {
                switch (Status) {
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.TargetExists:
                    case Status.TargetInvalid:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Processable {
            get {
                switch (Status) {
                    case Status.Processable:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Busy {
            get {
                switch (Status) {
                    case Status.Busy:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Paused {
            get {
                switch (Status) {
                    case Status.Paused:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Queued {
            get {
                switch (Status) {
                    case Status.Queued:
                    case Status.Paused:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Done {
            get {
                switch (Status) {
                    case Status.Finished:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool Error {
            get {
                switch (Status) {
                    case Status.ProjectMissing:
                    case Status.Unknown:
                    case Status.Error:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public int Percentage { get { return (WorkspaceItem.Project != null) ? WorkspaceItem.Project.Progress : 0; } }

        public bool SelectedForHandling {
            get {
                return _blnSelectedForHandling;
            }
            set {
                _blnSelectedForHandling = (
                    value == true &&
                    WorkspaceItem.Project != null &&
                    WorkspaceItem.Project.Status == ProjectStatus.Processable.ToString()
                );
                OnPropertyChanged("SelectedForHandling");
            }
        }
    }
}