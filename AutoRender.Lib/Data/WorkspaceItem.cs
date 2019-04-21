using System;
using System.Collections.Generic;

namespace AutoRender.Lib {
    public class WorkspaceItem {
        public static event WorkspaceItemUpdated Updated;
        public Guid ID { get; private set; }
        public Project Project { get; private set; }
        public VideoInfo New { get; private set; }
        public VideoInfo Final { get; private set; }


        public WorkspaceItem(Project pProject, VideoInfo pNew, VideoInfo pFinal) {
            ID = Guid.NewGuid();
            Project = pProject;
            New = pNew;
            Final = pFinal;
            if (Project != null) {
                Project.ProjectChanged += Project_ProjectChanged;
            }
        }

        void Project_ProjectChanged(object sender, EventArgs e) {
            Updated?.Invoke(this, new List<WorkspaceUpdatedEventArgs> { new WorkspaceUpdatedEventArgs(this, WorkspaceAction.Updated) });
        }


        #region Update Methods
        public bool UpdateProject(Project pProject) {
            if (pProject == null) {
                if (Project != null) {
                    Project.ProjectChanged -= Project_ProjectChanged;
                    Project = null; return true;
                }
            } else if (Project == null || !this.Project.Equals(pProject)) {
                Project = pProject;
                if (Project != null) {
                    Project.ProjectChanged += Project_ProjectChanged;
                }
                return true;
            }
            return false;
        }

        public bool UpdateFinal(VideoInfo pInfo) {
            if(Project != null) { Project.Reload(); }
            if (pInfo == null) {
                if (Final != null) {
                    Final = null; return true;
                }
            } else if (Final == null || !this.Final.Equals(pInfo)) {
                Final = pInfo;
                return true;
            }
            return false;
        }

        public bool UpdateNew(VideoInfo pInfo) {
            if (Project != null) { Project.Reload(); }
            if (pInfo == null && New != null) {
                New = null; return true;
            } else if (New != null && !this.New.Equals(pInfo)) {
                New = pInfo; return true;
            }
            return false;
        }
        #endregion

        #region Compare/Equality Methods
        public bool Equals(WorkspaceItem pItem) {
            if (pItem == null) { return false; }

            //check for null first
            if (
                this.Final == null && pItem.Final != null ||
                this.New == null && pItem.New != null ||
                this.Project == null && pItem.Project != null
            ) {
                return false;
            }
            return (
                this.ID.Equals(pItem.ID) &&
                Project.Equals(this.Project, pItem.Project) &&
                VideoInfo.Equals(this.New, pItem.New) &&
                VideoInfo.Equals(this.Final, pItem.Final)
             );
        }

        public override int GetHashCode() {
            return this.GetHashCodeFromFields(this.Final, this.ID, this.New, this.Project);
        }
        public override bool Equals(object obj) {
            return this.Equals(obj as WorkspaceItem);
        }
        #endregion
    }
}