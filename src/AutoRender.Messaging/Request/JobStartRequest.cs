using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobStartRequest : RequestMessage {
        public Guid ProjectID { get; set; }

        public JobStartRequest() : base() {
        }

        public JobStartRequest(Guid pProjectID) : base() {
            ProjectID = pProjectID;
        }
    }
}