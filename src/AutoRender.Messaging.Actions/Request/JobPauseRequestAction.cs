using AutoRender.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Action.Request {

    public class JobPause : RequestAction<JobPauseRequest, ACKResponse> {

        public JobPause(IClient pClient, Messaging.Request.JobPauseRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWSItem = WorkspaceFactory.Get("").Get(Request.ProjectID);
            if (objWSItem != null && objWSItem.Project != null) {
                objWSItem.Project.Pause();
                return new ACKResponse(Request);
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}