using WebSocketMessaging;
using WebSocketMessaging.Action;
using AutoRender.Lib;

namespace AutoRender.Messaging.Action.Request {
    public class JobStart : RequestAction<Messaging.Request.JobStart> {
        public JobStart(Client pClient, Messaging.Request.JobStart pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            var objWsItem = Workspace.Get(Request.ProjectID);
            if (objWsItem != null) {
                if (objWsItem.Project != null) {
                    objWsItem.Project.Start();
                    return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
                }
            }
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.ProjectNotFound);
        }
    }
}