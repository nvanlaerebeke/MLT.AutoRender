using System;
using System.Reflection;
using AutoRender.Data;
using AutoRender.MLT;
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
                if (Project != null) {
                    Project = pProject;
                    Updated?.Invoke(this, this);
                    return true;
                }
            } else if (!pProject.Equals(Project)) {
                Project = pProject;
                Updated?.Invoke(this, this);
                return true;
            }
            return false;
        }

        public bool UpdateFinal(VideoInfo pFinal) {
            if (Project != null) { Project.Reload(); }
            if (pFinal == null) {
                if (Final != null) {
                    Final = pFinal;
                    Updated?.Invoke(this, this);
                    return true;
                }
            } else if (!pFinal.Equals(Final)) {
                Final = pFinal;
                Updated.Invoke(this, this);
                return true;
            }
            return false;
        }

        public bool UpdateNew(VideoInfo pNew) {
            if (Project != null) { Project.Reload(); }
            if (pNew == null) {
                if (New != null) {
                    New = pNew;
                    Updated?.Invoke(this, this);
                    return true;
                }
            } else if (!pNew.Equals(New)) {
                New = pNew;
                Updated?.Invoke(this, this);
                return true;
            }
            return false;
        }

        public Data.WorkspaceItem GetWorkspaceItem() {
            return new Data.WorkspaceItem(ID) {
                Project = Project?.GetProject(),
                Final = Final,
                New = New
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