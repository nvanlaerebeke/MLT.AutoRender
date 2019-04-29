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

    public class UpdateProjectTargetRequestAction : RequestAction<UpdateProjectTargetRequest, ACKResponse> {

        public UpdateProjectTargetRequestAction(IClient pClient, UpdateProjectTargetRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                objWsItem.Project.TargetName = Request.ProjectTargetName;

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
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"Project {Request.ItemID} not found"));
        }
    }
}