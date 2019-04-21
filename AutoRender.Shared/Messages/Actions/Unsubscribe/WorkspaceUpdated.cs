using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.UnSubscribe {
    public class WorkspaceUpdated : RequestAction<Messaging.UnSubscribe.WorkspaceUpdated> {
        public WorkspaceUpdated(Client pClient, Messaging.UnSubscribe.WorkspaceUpdated pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            Client.MessageProvider.GetSubscriptionHandler(Request).UnSubscribe(Client);
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
        }
    }
}