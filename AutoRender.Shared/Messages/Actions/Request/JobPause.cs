using AutoRender.Lib;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class JobPause : RequestAction<Messaging.Request.JobPause> {
        public JobPause(Client pClient, Messaging.Request.JobPause pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            var objWsItem = Workspace.Get(Request.ProjectID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Pause();
                return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
            }
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.ProjectNotFound);
        }
    }
}