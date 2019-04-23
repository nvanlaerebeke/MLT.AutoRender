using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobStopRequest : RequestMessage {
        public Guid ProjectID { get; set; }

        public JobStopRequest() : base() {
        }

        public JobStopRequest(Guid pID) : base() {
            ProjectID = pID;
        }
    }
}