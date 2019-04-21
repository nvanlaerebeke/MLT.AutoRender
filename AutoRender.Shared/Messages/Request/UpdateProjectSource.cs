using System;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class UpdateProjectSource : RequestMessage {
        public Guid ProjectID { get; set; }
        public string ProjectSourceName { get; set; }
        public UpdateProjectSource() : base() { }
        public UpdateProjectSource(Guid pID, string pName) : base() {
            ProjectID = pID;
            ProjectSourceName = pName;
        }
        public override byte GetCode() {
            return (byte)MessageCode.UpdateProjectSource;
        }
    }
}