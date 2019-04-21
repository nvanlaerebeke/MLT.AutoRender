using System;
using Newtonsoft.Json;
using WebSocketMessaging;
using System.Collections.Generic;

namespace AutoRender.Messaging.Response {
    public class GetStatus: ResponseMessage {

        public List<WorkspaceItem> WorkspaceItems { get; set; }

        [JsonConstructor]
        public GetStatus() { }

        public GetStatus(RequestMessage pMessage, ResponseCode pStatus) : base(pMessage, pStatus) { }

        public GetStatus(RequestMessage pMessage, List<WorkspaceItem> pItems) : base(pMessage, ResponseCode.Success) {
            WorkspaceItems = pItems;
        }

        public override byte GetCode() {
            return (byte)MessageCode.GetStatus;
        }
    }
}