using AutoRender.Data;
using AutoRender.MLT;
using System;
using System.Collections.Generic;

namespace AutoRender.Workspace {

    public class WorkspaceItem {

        public static event EventHandler<List<WorkspaceUpdatedEventArgs>> Updated;

        public Guid ID { get; private set; }
        public MLTProject Project { get; private set; }
        public VideoInfo New { get; private set; }
        public VideoInfo Final { get; private set; }

        public WorkspaceItem(MLTProject pProject, VideoInfo pNew, VideoInfo pFinal) {
            ID = Guid.NewGuid();
            Project = pProject;
            New = pNew;
            Final = pFinal;
            if (Project != null) {
                Project.ProjectChanged += Project_ProjectChanged;
            }
        }

        private void Project_ProjectChanged(object sender, EventArgs e) {
            Updated?.Invoke(this, new List<WorkspaceUpdatedEventArgs> { new WorkspaceUpdatedEventArgs(new Data.WorkspaceItem() {
                Final = Final,
                ID = ID,
                New = New,
                Project = Project.GetProject()
            }, WorkspaceAction.Updated) });
        }

        #region Update Methods

        public bool UpdateProject(MLTProject pProject) {
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
            if (Project != null) { Project.Reload(); }
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

        public Data.WorkspaceItem GetWorkspaceItem() {
            return new Data.WorkspaceItem() {
                Project = Project.GetProject(),
                Final = Final,
                New = New,
                ID = ID
            };
        }
        #endregion Update Methods

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
                MLTProject.Equals(this.Project, pItem.Project) &&
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

        #endregion Compare/Equality Methods
    }
}