using AutoRender.Data;
using AutoRender.MLT;
using System;
using System.Reflection;
using log4net;

namespace AutoRender.Workspace {

    public class WorkspaceItem {
        private static readonly log4net.ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<WorkspaceItem> Updated;

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
                Project.ProjectChanged += (sender, e) => {
                    Log.Info("WorkspaceItem was change - notify workspace");
                    switch (Project.Status) {
                        case ProjectStatus.Error:
                        case ProjectStatus.Finished:
                        case ProjectStatus.SourceInvalid:
                        case ProjectStatus.SourceMissing:
                        case ProjectStatus.TargetExists:
                        case ProjectStatus.TargetInvalid:
                            if (Project.TargetExists) {
                                Final = Project.VideoInfoProvider.Get(Project.TargetPath);
                            }
                            break;
                    }
                    Updated?.Invoke(this, this);
                };
            }
        }


        #region Update Methods

        public bool UpdateProject(MLTProject pProject) {
            if (pProject == null) {
                Project = null; return true;
            } else if (Project == null || !this.Project.Equals(pProject)) {
                Project = pProject;
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
            } else if (Final == null || !Final.Equals(pInfo)) {
                Final = pInfo;
                return true;
            }
            return false;
        }

        public bool UpdateNew(VideoInfo pInfo) {
            if (Project != null) { Project.Reload(); }
            if (pInfo == null && New != null) {
                New = null; return true;
            }
            if (New != null && !New.Equals(pInfo)) {
                New = pInfo;
                Project.SourcePath = pInfo.Path;
                return true;
            }
            return false;
        }

        public Data.WorkspaceItem GetWorkspaceItem() {
            return new Data.WorkspaceItem() {
                Project = Project?.GetProject(),
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
                Final == null && pItem.Final != null ||
                New == null && pItem.New != null ||
                Project == null && pItem.Project != null
            ) {
                return false;
            }
            return (
                ID.Equals(pItem.ID) &&
                Equals(Project, pItem.Project) &&
                Equals(New, pItem.New) &&
                Equals(Final, pItem.Final)
             );
        }

        public override int GetHashCode() {
            return this.GetHashCodeFromFields(Final, ID, New, Project);
        }

        public override bool Equals(object obj) {
            return Equals(obj as WorkspaceItem);
        }

        #endregion Compare/Equality Methods
    }
}