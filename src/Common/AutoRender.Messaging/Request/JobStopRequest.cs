using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobStopRequest : RequestMessage {
        public Guid ItemID { get; set; }

        public JobStopRequest() : base() {
        }

        public JobStopRequest(Guid pID) : base() {
            ItemID = pID;
        }
    }
}