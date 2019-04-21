using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class UpdateProjectTarget : RequestAction<Messaging.Request.UpdateProjectTarget> {
        public UpdateProjectTarget(Client pClient, Messaging.Request.UpdateProjectTarget pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            lock (Lib.Workspace.WorkspaceItems) {
                var objWsItem = Lib.Workspace.Get(Request.ProjectID);
                if (objWsItem != null) {
                    objWsItem.Project.TargetName = Request.ProjectTargetName;
                    return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
                }
            }
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.ProjectNotFound);
        }
    }
}