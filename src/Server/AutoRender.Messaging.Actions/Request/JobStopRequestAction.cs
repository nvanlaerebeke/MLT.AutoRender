using System.Collections.Generic;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class JobStopRequestAction : RequestAction<JobStopRequest, GetStatusResponse> {

        public JobStopRequestAction(IClient pClient, JobStopRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Stop();
                return new GetStatusResponse(Request, new List<Data.WorkspaceItem>() { objWsItem.GetWorkspaceItem() });
            }
            return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}