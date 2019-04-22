using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Subscription.Messaging.UnSubscribe;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using Mitto.Subscription.Messaging;

namespace AutoRender.Subscription.Messaging.Action.UnSubscribe {

    public class WorkspaceUpdatedUnSubscribeAction : RequestAction<WorkspaceUpdatedUnSubscribe, ACKResponse> {

        public WorkspaceUpdatedUnSubscribeAction(IClient pClient, WorkspaceUpdatedUnSubscribe pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            if (new SubscriptionClient<WorkspaceUpdatedHandler>(Client).UnSub(Request)) {
                return new ACKResponse(Request);
            }
            return new ACKResponse(Request,
                new ResponseStatus(
                    ResponseState.Error,
                    $"Failed to subscribe to Workspace"
                )
            );
        }
    }
}