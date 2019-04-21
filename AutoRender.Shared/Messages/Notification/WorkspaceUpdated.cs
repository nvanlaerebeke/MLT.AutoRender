using System.Collections.Generic;
namespace AutoRender.Messaging.Notification {
    public class WorkspaceUpdated : WebSocketMessaging.NotificationMessage {
        public List<WorkspaceItemUpdate> Updates { get; set; }
        public WorkspaceUpdated() : base() { }

        public WorkspaceUpdated(List<WorkspaceItemUpdate> pUpdates) : base() {
            Updates = pUpdates;
        }

        public override byte GetCode() {
            return (byte)MessageCode.WorkspaceUpdated;
        }
    }
}
