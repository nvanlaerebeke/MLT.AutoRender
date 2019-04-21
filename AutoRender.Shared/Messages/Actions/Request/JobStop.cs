using AutoRender.Lib;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class JobStop : RequestAction<Messaging.Request.JobStop> {
        public JobStop(Client pClient, Messaging.Request.JobStop pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            var objWsItem = Workspace.Get(Request.ProjectID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Stop();
                return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
            } else {
                return new WebSocketMessaging.Response.ACK(Request, ResponseCode.ProjectNotFound);
            }
        }
    }
}