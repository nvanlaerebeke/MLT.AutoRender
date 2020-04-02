using System;
using AutoRender.Messaging.Request;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;

namespace AutoRender.Messaging.Actions.Request {

    public class ReloadRequestAction : RequestAction<ReloadRequest, ACKResponse> {
        public static event EventHandler ReloadRequested;

        public ReloadRequestAction(IClient pClient, ReloadRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            ReloadRequested?.Invoke(this, new EventArgs());
            return new ACKResponse(Request);
        }
    }
}