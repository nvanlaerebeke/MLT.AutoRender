using System.Collections.Generic;
using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Subscription.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using Mitto.Subscription.Messaging;

namespace AutoRender.Messaging.Action.Request {

    public class JobStopRequestAction : RequestAction<JobStopRequest, ACKResponse> {

        public JobStopRequestAction(IClient pClient, Messaging.Request.JobStopRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ProjectID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Stop();

                new SubscriptionClient<WorkspaceUpdatedHandler>(Client).Notify(new SendWorkspaceUpdatedRequest(
                    new List<WorkspaceUpdatedEventArgs>() {
                        new WorkspaceUpdatedEventArgs(
                            objWsItem.GetWorkspaceItem(),
                            WorkspaceAction.Updated
                        )
                    }
                ));

                return new ACKResponse(Request);
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}