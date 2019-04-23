using System;

namespace AutoRender {
    public partial class WorkspaceItemViewModel {
        public bool ShowCreateProject {
            get {
                return String.IsNullOrEmpty(ProjectName);
            }
        }

        public string ProjectName {
            get {
                if (WorkspaceItem.Project != null) {
                    return WorkspaceItem.Project.ProjectName;
                }
                return "";
            }
        }
    }
}