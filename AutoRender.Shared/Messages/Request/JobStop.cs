using System;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class JobStop : RequestMessage {
        public Guid ProjectID { get; set; }
        public JobStop() : base() { }
        public JobStop(Guid pID) : base() {
            ProjectID = pID;
        }
        public override byte GetCode() {
            return (byte)MessageCode.JobStop;
        }
    }
}