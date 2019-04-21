using System;
using WebSocketMessaging;

namespace AutoRender.Messaging.Request {
    public class UpdateProjectTarget : RequestMessage {
        public Guid ProjectID { get; set; }
        public string ProjectTargetName { get; set; }
        public UpdateProjectTarget() : base() { }
        public UpdateProjectTarget(Guid pID, string pName) : base() {
            ProjectID = pID;
            ProjectTargetName = pName;
        }
        public override byte GetCode() {
            return (byte)MessageCode.UpdateProjectTarget;
        }
    }
}