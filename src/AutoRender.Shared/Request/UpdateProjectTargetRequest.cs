using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class UpdateProjectTargetRequest : RequestMessage {
        public Guid ProjectID { get; set; }
        public string ProjectTargetName { get; set; }

        public UpdateProjectTargetRequest() : base() {
        }

        public UpdateProjectTargetRequest(Guid pID, string pName) : base() {
            ProjectID = pID;
            ProjectTargetName = pName;
        }
    }
}