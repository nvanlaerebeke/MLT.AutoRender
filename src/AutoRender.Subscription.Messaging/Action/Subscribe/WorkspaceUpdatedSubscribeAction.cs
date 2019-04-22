using AutoRender.Subscription.Messaging.Handlers;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using Mitto.Subscription.Messaging;

namespace AutoRender.Subscription.Messaging.Action.Subscribe {

    public class WorkspaceUpdatedSubscribeAction : RequestAction<WorkspaceUpdatedSubscribe, ACKResponse> {

        public WorkspaceUpdatedSubscribeAction(IClient pClient, WorkspaceUpdatedSubscribe pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            if (new SubscriptionClient<WorkspaceUpdatedHandler>(Client).Sub(Request)) {
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