using System.Collections.Generic;
namespace AutoRender.Messaging.Event {
    public class WorkspaceUpdated : WebSocketMessaging.EventMessage {
        public List<WorkspaceItemUpdate> Updates { get; set; }
        public WorkspaceUpdated() : base() { }
        public WorkspaceUpdated(List<WorkspaceItemUpdate> pUpdates, bool pForwarded) : base(pForwarded) {
            Updates = pUpdates;
        }

        public override byte GetCode() {
            return (byte)MessageCode.WorkspaceUpdated;
        }
    }
}
