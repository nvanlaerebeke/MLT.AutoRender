using AutoRender.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Action.Request {

    public class JobStartRequestAction : RequestAction<JobStartRequest, ACKResponse> {

        public JobStartRequestAction(IClient pClient, JobStartRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ProjectID);
            if (objWsItem != null) {
                if (objWsItem.Project != null) {
                    objWsItem.Project.Start();
                    return new ACKResponse(Request);
                }
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}