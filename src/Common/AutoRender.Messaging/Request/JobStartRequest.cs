using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobStartRequest : RequestMessage {
        public Guid ItemID { get; set; }

        public JobStartRequest() : base() {
        }

        public JobStartRequest(Guid pProjectID) : base() {
            ItemID = pProjectID;
        }
    }
}