using System;
using AutoRender.Messaging.Request;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Actions.Request {

    public class ReloadRequestAction : NotificationAction<ReloadRequest> {

        public static event EventHandler ReloadRequested;

        public ReloadRequestAction(IClient pClient, ReloadRequest pRequest) : base(pClient, pRequest) {
        }

        public override void Start() {
            ReloadRequested?.Invoke(this, new EventArgs());
        }
    }
}