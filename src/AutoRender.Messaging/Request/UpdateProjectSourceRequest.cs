using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class UpdateProjectSourceRequest : RequestMessage {
        public Guid ItemID { get; set; }
        public string ProjectSourceName { get; set; }

        public UpdateProjectSourceRequest() : base() {
        }

        public UpdateProjectSourceRequest(Guid pID, string pName) : base() {
            ItemID = pID;
            ProjectSourceName = pName;
        }
    }
}