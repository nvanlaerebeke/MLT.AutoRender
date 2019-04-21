using System.Collections.Generic;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class Reload : RequestMessage {
        public Reload() { }

        public override byte GetCode() {
            return (byte)MessageCode.Reload;
        }
    }
}