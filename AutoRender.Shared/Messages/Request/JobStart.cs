using System;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class JobStart : RequestMessage {
        public Guid ProjectID { get; set; }
        public JobStart() : base() { }
        public JobStart(Guid pProjectID) : base() {
            ProjectID = pProjectID;
        }
        public override byte GetCode() {
            return (byte)MessageCode.JobStart;
        }
    }
}