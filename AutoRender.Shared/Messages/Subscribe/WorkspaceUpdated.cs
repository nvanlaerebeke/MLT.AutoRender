using WebSocketMessaging;

namespace AutoRender.Messaging.Subscribe {
    public class WorkspaceUpdated : SubscribeMessage {
        public WorkspaceUpdated(): base() { }

        public override byte GetCode() {
            return (byte)MessageCode.WorkspaceUpdated;
        }
    }
}
