using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class GetStatus : RequestAction<Messaging.Request.GetStatus> {
        public GetStatus(Client pClient, Messaging.Request.GetStatus pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            lock (Lib.Workspace.WorkspaceItems) {
                if (Request.WorkspaceItemIDs.Count == 0) { // -- return everything
                    Lib.Workspace.WorkspaceItems.ForEach(i => lstItems.Add(new WorkspaceItem(i)));
                } else { // -- return only requested project ids
                    Lib.Workspace.WorkspaceItems.ForEach(i => {
                        if (Request.WorkspaceItemIDs.Contains(i.ID.ToString())) {
                            lstItems.Add(new WorkspaceItem(i));
                        }
                    });
                }
            }
            return new Response.GetStatus(Request, lstItems);
        }
    }
}