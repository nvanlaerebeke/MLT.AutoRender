using System;
using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Notification {
    public delegate void WorkspaceItemsUpdated(object sender, List<WorkspaceItemUpdate> pUpdates);
    public class WorkspaceUpdated : RequestAction<Messaging.Notification.WorkspaceUpdated> {
        public static event WorkspaceItemsUpdated Updated; 
        public WorkspaceUpdated(Client pClient, Messaging.Notification.WorkspaceUpdated pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            Updated?.Invoke(this, Request.Updates);
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
        }
    }
}