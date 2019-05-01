using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobPauseRequest : RequestMessage {
        public Guid ItemID { get; set; }

        public JobPauseRequest() : base() {
        }

        public JobPauseRequest(Guid pID) : base() {
            ItemID = pID;
        }
    }
}