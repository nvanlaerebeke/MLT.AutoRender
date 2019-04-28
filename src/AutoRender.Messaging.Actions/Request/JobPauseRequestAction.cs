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

        public JobPauseRequestAction(IClient pClient, Messaging.Request.JobPauseRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWSItem = WorkspaceFactory.Get().Get(Request.ProjectID);
            if (objWSItem != null && objWSItem.Project != null) {
                objWSItem.Project.Pause();

                new SubscriptionClient<WorkspaceUpdatedHandler>(Client).Notify(new SendWorkspaceUpdatedRequest(
                    new List<Data.WorkspaceUpdatedEventArgs>() {
                        new Data.WorkspaceUpdatedEventArgs(
                            objWSItem.GetWorkspaceItem(),
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