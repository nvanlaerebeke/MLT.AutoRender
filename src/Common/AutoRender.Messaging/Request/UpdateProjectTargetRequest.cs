using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class UpdateProjectTargetRequest : RequestMessage {
        public Guid ItemID { get; set; }
        public string ProjectTargetName { get; set; }

        public UpdateProjectTargetRequest() : base() {
        }

        public UpdateProjectTargetRequest(Guid pID, string pName) : base() {
            ItemID = pID;
            ProjectTargetName = pName;
        }
    }
}