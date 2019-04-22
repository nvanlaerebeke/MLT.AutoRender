using AutoRender.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Action.Request {

    public class UpdateProjectTarget : RequestAction<UpdateProjectTargetRequest, ACKResponse> {

        public UpdateProjectTarget(IClient pClient, UpdateProjectTargetRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get("").Get(Request.ProjectID);
            if (objWsItem != null) {
                objWsItem.Project.TargetName = Request.ProjectTargetName;
                return new ACKResponse(Request);
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"Project {Request.ProjectID} not found"));
        }
    }
}