using System.Collections.Generic;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.MLT;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class JobStartRequestAction : RequestAction<JobStartRequest, GetStatusResponse> {

        public JobStartRequestAction(IClient pClient, JobStartRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                MeltJobScheduler.GetScheduler().Schedule(objWsItem.Project.Job);
                return new GetStatusResponse(Request, new List<Data.WorkspaceItem>() { objWsItem.GetWorkspaceItem() });
            }
            return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}