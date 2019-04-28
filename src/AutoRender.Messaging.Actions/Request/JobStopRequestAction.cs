using AutoRender.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Action.Request {

    public class JobStopRequestAction : RequestAction<JobStopRequest, ACKResponse> {

        public JobStopRequestAction(IClient pClient, Messaging.Request.JobStopRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ProjectID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Stop();
                return new ACKResponse(Request);
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}