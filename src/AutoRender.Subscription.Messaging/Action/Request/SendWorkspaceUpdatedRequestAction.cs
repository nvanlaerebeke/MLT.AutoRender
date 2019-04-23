using AutoRender.Data;
using AutoRender.Subscription.Messaging.Request;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using System;
using System.Collections.Generic;

namespace AutoRender.Subscription.Messaging.Action.Request {

    public delegate void WorkspaceUpdated(IClient pClient, List<WorkspaceUpdatedEventArgs> pUpdates);

    public class SendWorkspaceUpdatedRequestAction : RequestAction<
        SendWorkspaceUpdatedRequest,
        ACKResponse
    > {

        public static event WorkspaceUpdated WorkspaceUpdated;

        public SendWorkspaceUpdatedRequestAction(IClient pClient, SendWorkspaceUpdatedRequest pMessage) : base(pClient, pMessage) {
        }

        public override ACKResponse Start() {
            WorkspaceUpdated?.Invoke(Client, Request.Updates);
            return new ACKResponse();
        }
    }
}