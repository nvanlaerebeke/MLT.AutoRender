using System.IO;
using System.Collections.Generic;
using WebSocketMessaging;
using WebSocketMessaging.Action;

namespace AutoRender.Messaging.Action.Request {
    public class UpdateProjectSource : RequestAction<Messaging.Request.UpdateProjectSource> {
        public UpdateProjectSource(Client pClient, Messaging.Request.UpdateProjectSource pRequest) : base(pClient, pRequest) { }

        public override ResponseMessage Start() {
            lock (Lib.Workspace.WorkspaceItems) {
                var objWsItem = Lib.Workspace.Get(Request.ProjectID);
                if (objWsItem != null) {
                    string strNewPath = Path.Combine(Settings.NewDirectory, Request.ProjectSourceName);
                    if(File.Exists(strNewPath)) {
                        objWsItem.Project.SourcePath = strNewPath;
                        return new WebSocketMessaging.Response.ACK(Request, ResponseCode.Success);
                    } 
                    return new WebSocketMessaging.Response.ACK(Request, ResponseCode.FileNotFound);
                }
            }
            return new WebSocketMessaging.Response.ACK(Request, ResponseCode.ProjectNotFound);
        }
    }
}