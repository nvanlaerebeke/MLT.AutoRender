using System.Collections.Generic;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class GetStatus: RequestMessage {
        public List<string> WorkspaceItemIDs { get; set; }
        public GetStatus() : base() { WorkspaceItemIDs = new List<string>(); }
        public GetStatus(string pProjectID) : base() { WorkspaceItemIDs = new List<string> { pProjectID }; }
        public GetStatus(List<string> pProjectIDs) : base() { WorkspaceItemIDs = pProjectIDs; }

        public override byte GetCode() {
            return (byte)MessageCode.GetStatus;
        }
    }
}