using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using System.Collections.Generic;

namespace AutoRender.Messaging.Action.Request {

    public class GetStatus : RequestAction<GetStatusRequest, GetStatusResponse> {

        public GetStatus(IClient pClient, GetStatusRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            Workspace.WorkspaceFactory.Get("").WorkspaceItems.ForEach(i =>
                lstItems.Add(new WorkspaceItem() {
                    ID = i.ID,
                    Project = i.Project.GetProject(),
                    New = i.New,
                    Final = i.Final
                })
            );
            return new GetStatusResponse(Request, lstItems);
        }
    }
}