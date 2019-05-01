using System;
using System.IO;

namespace AutoRender {
    public partial class WorkspaceItemViewModel {
        public bool CanStart {
            get {
                switch (Status) {
                    case Status.Processable:
                    case Status.Paused:
                        return true;
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
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
        public bool CanStop {
            get {
                switch (Status) {
                    case Status.Busy:
                    case Status.Paused:
                        return true;
                    case Status.Processable:
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.TargetExists:
                    case Status.TargetInvalid:
                    case Status.ProjectMissing:
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
        public bool CanPause {
            get {
                switch (Status) {
                    case Status.Busy:
                        return true;
                    case Status.Processable:
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.TargetExists:
                    case Status.TargetInvalid:
                    case Status.ProjectMissing:
                    case Status.Queued:
                    case Status.Finished:
                    case Status.Unknown:
                    case Status.Error:
                    case Status.Updating:
                    case Status.Paused:
                    default:
                        return false;
                }
            }
        }
        public bool CanEditTargetName {
            get {
                if(TargetNameIsEditing) { return false; }
                switch (Status) {
                    case Status.Processable:
                    case Status.TargetExists:
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.TargetInvalid:
                        return true;
                    case Status.Busy:
                    case Status.ProjectMissing:
                    case Status.Queued:
                    case Status.Finished:
                    case Status.Unknown:
                    case Status.Error:
                    case Status.Updating:
                    case Status.Paused:
                    default:
                        return false;
                }
            }
        }
        public bool CanEditSourceName{
            get {
                if(SourceNameIsEditing) { return false; }
                switch (Status) {
                    case Status.SourceMissing:
                    case Status.SourceInvalid:
                    case Status.Processable:
                    case Status.TargetInvalid:
                        return true;
                    case Status.TargetExists:
                    case Status.Busy:
                    case Status.ProjectMissing:
                    case Status.Queued:
                    case Status.Finished:
                    case Status.Unknown:
                    case Status.Error:
                    case Status.Updating:
                    case Status.Paused:
                    default:
                        return false;
                }
            }
        }

		public bool CanOpenShotcut {
			get {
				string strPath = Path.Combine(Settings.ProjectPath, Path.ChangeExtension(this.ProjectName, ".mlt"));
				return (!String.IsNullOrEmpty(this.ProjectName) && File.Exists(strPath));
			}
		}
	}
}
