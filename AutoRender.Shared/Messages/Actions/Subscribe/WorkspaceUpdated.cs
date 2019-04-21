using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Subscribe {
    public class WorkspaceUpdated : RequestAction<Messaging.Subscribe.WorkspaceUpdated> {
        public WorkspaceUpdated(Client pClient, Messaging.Subscribe.WorkspaceUpdated pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            Client.MessageProvider.GetSubscriptionHandler(Request).Subscribe(Client);
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
        }
    }
}