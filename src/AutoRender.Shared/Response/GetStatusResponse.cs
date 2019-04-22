using AutoRender.Data;
using Mitto.IMessaging;
using Mitto.Messaging;
using System.Collections.Generic;

namespace AutoRender.Messaging.Response {

    public class GetStatusResponse : ResponseMessage {
        public List<WorkspaceItem> WorkspaceItems { get; set; }

        public GetStatusResponse() {
        }

        public GetStatusResponse(RequestMessage pMessage, ResponseStatus pStatus) : base(pMessage, pStatus) {
        }

        public GetStatusResponse(RequestMessage pMessage, List<WorkspaceItem> pItems) : base(pMessage, new ResponseStatus()) {
            WorkspaceItems = pItems;
        }
    }
}