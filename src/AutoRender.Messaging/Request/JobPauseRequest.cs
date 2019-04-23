using Mitto.Messaging;
using System;

namespace AutoRender.Messaging.Request {

    public class JobPauseRequest : RequestMessage {
        public Guid ProjectID { get; set; }

        public JobPauseRequest() : base() {
        }

        public JobPauseRequest(Guid pID) : base() {
            ProjectID = pID;
        }
    }
}