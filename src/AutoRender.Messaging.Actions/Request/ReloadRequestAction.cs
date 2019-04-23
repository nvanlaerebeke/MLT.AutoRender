using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using System.Collections.Generic;

namespace AutoRender.Messaging.Action.Request {

    public class Reload : RequestAction<ReloadRequest, GetStatusResponse> {

        public Reload(IClient pClient, ReloadRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            Workspace.WorkspaceFactory.Get("").Reload();

            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            Workspace.WorkspaceFactory.Get("").WorkspaceItems.ForEach(i =>
                lstItems.Add(new WorkspaceItem() {
                    ID = i.ID,
                    Project = new Project() {
                        Progress = i.Project.Progress,
                        ProjectName = i.Project.Name,
                        SourceExists = i.Project.SourceExists,
                        SourceIsValid = i.Project.SourceIsValid,
                        SourceName = System.IO.Path.GetFileName(i.Project.SourcePath),
                        StartTime = i.Project.StartTime,
                        Status = i.Project.Status,
                        TargetExists = i.Project.TargetExists,
                        TargetIsValid = i.Project.TargetIsValid,
                        TargetName = i.Project.TargetName,
                        TimeTaken = i.Project.TimeTaken
                    },
                    New = i.New,
                    Final = i.Final
                })
            );
            return new GetStatusResponse(Request, lstItems);
        }
    }
}