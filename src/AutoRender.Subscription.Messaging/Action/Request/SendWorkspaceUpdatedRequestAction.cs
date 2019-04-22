using AutoRender.Subscription.Messaging.Request;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using System;

namespace AutoRender.Subscription.Messaging.Action.Request {

    public class SendWorkspaceUpdatedRequestAction : RequestAction<
        SendWorkspaceUpdatedRequest,
        ACKResponse
    > {

        public static event EventHandler<IClient> WorkspaceUpdated;

        public SendWorkspaceUpdatedRequestAction(IClient pClient, SendWorkspaceUpdatedRequest pMessage) : base(pClient, pMessage) {
        }

        public override ACKResponse Start() {
            WorkspaceUpdated?.Invoke(this, Client);
            return new ACKResponse();
        }
    }
}