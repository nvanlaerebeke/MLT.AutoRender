using AutoRender.Data;
using Mitto.Messaging;
using System.Collections.Generic;

namespace AutoRender.Subscription.Messaging.Request {

    public class SendWorkspaceUpdatedRequest : RequestMessage {
        public List<WorkspaceUpdatedEventArgs> Updates { get; set; }

        public SendWorkspaceUpdatedRequest(List<WorkspaceUpdatedEventArgs> pUpdates) {
            Updates = pUpdates;
        }
    }
}