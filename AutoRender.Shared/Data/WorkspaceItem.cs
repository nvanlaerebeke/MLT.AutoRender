using System;
using Newtonsoft.Json;

namespace AutoRender.Messaging {
    public class WorkspaceItem {
        public Guid ID { get; set; }
        public Project Project { get; set; }
        public VideoInfo New { get; set; }
        public VideoInfo Final { get; set; }

        public WorkspaceItem(Lib.WorkspaceItem pWorkspaceItem) {
            ID = pWorkspaceItem.ID;
            Project = (pWorkspaceItem.Project != null) ? new Project(pWorkspaceItem.Project) : null;
            New = (pWorkspaceItem.New != null) ? new VideoInfo(pWorkspaceItem.New) : null;
            Final = (pWorkspaceItem.Final != null) ? new VideoInfo(pWorkspaceItem.Final) : null;
        }

        [JsonConstructor]
        public WorkspaceItem(Guid pID, Project pProject, VideoInfo pNew, VideoInfo pFinal) {
            ID = pID;
            Project = pProject;
            New = pNew;
            Final = pFinal;
        }
    }
}