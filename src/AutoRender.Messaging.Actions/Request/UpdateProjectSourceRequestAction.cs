using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using System.IO;

namespace AutoRender.Messaging.Action.Request {

    public class UpdateProjectSource : RequestAction<UpdateProjectSourceRequest, ACKResponse> {

        public UpdateProjectSource(IClient pClient, UpdateProjectSourceRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objItem = WorkspaceFactory.Get("").Get(Request.ProjectID);
            if (objItem != null) {
                string strNewPath = Path.Combine(Settings.NewDirectory, Request.ProjectSourceName);
                if (File.Exists(strNewPath)) {
                    objItem.Project.SourcePath = strNewPath;
                    return new ACKResponse(Request);
                }
                return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"File {strNewPath} not found"));
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"Project {Request.ProjectID} not found"));
        }
    }
}