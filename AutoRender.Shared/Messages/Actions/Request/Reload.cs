using System.Collections.Generic;
using AutoRender.Lib;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class Reload : RequestAction<Messaging.Request.Reload> {
        public Reload(Client pClient, Messaging.Request.Reload pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            Workspace.Reload();

            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            lock (Lib.Workspace.WorkspaceItems) {
                Lib.Workspace.WorkspaceItems.ForEach(i => lstItems.Add(new WorkspaceItem(i)));
            }
            return new Response.GetStatus(Request, lstItems);
        }
    }
}