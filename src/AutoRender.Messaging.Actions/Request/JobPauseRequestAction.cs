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

    public class JobPauseRequestAction : RequestAction<JobPauseRequest, ACKResponse> {

        public JobPauseRequestAction(IClient pClient, JobPauseRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.Pause();

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