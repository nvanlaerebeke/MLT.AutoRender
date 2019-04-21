using WebSocketMessaging;

namespace AutoRender.Messaging.UnSubscribe {
    public class WorkspaceUpdated : UnSubscribeMessage {
        public WorkspaceUpdated(): base() { }

        public override byte GetCode() {
            return (byte)MessageCode.WorkspaceUpdated;
        }
    }
}
