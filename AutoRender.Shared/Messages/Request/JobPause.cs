using System;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class JobPause : RequestMessage {
        public Guid ProjectID { get; set; }
        public JobPause() : base() { }
        public JobPause(Guid pID) : base() {
            ProjectID = pID;
        }
        public override byte GetCode() {
            return (byte)MessageCode.JobPause;
        }
    }
}