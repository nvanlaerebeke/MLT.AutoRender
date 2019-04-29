using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using System.Collections.Generic;

namespace AutoRender.Messaging.Action.Request {

    public class ReloadRequestAction : RequestAction<ReloadRequest, GetStatusResponse> {

        public ReloadRequestAction(IClient pClient, ReloadRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            Workspace.WorkspaceFactory.Get().ReLoad();

            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            foreach(var objKvp in Workspace.WorkspaceFactory.Get().WorkspaceItems) {
                lstItems.Add(objKvp.Value.GetWorkspaceItem());
            }
            return new GetStatusResponse(Request, lstItems);
        }
    }
}