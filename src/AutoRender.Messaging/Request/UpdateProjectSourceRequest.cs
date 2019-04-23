using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class UpdateProjectSourceRequest : RequestMessage {
        public Guid ProjectID { get; set; }
        public string ProjectSourceName { get; set; }

        public UpdateProjectSourceRequest() : base() {
        }

        public UpdateProjectSourceRequest(Guid pID, string pName) : base() {
            ProjectID = pID;
            ProjectSourceName = pName;
        }
    }
}