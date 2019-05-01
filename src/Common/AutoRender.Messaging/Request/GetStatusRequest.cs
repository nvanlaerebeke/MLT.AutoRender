using Mitto.Messaging;
using System.Collections.Generic;

namespace AutoRender.Messaging.Request {

    public class GetStatusRequest : RequestMessage {
        public List<string> WorkspaceItemIDs { get; set; }

        public GetStatusRequest() : base() {
            WorkspaceItemIDs = new List<string>();
        }

        public GetStatusRequest(string pProjectID) : base() {
            WorkspaceItemIDs = new List<string> { pProjectID };
        }

        public GetStatusRequest(List<string> pProjectIDs) : base() {
            WorkspaceItemIDs = pProjectIDs;
        }
    }
}