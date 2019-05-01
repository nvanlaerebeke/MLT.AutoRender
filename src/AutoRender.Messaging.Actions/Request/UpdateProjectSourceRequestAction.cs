using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Subscription.Messaging.Request;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using Mitto.Messaging.Response;
using Mitto.Subscription.Messaging;
using System.Collections.Generic;
using System.IO;

namespace AutoRender.Messaging.Action.Request {

    public class UpdateProjectSourceRequestAction : RequestAction<UpdateProjectSourceRequest, GetStatusResponse> {

        public UpdateProjectSourceRequestAction(IClient pClient, UpdateProjectSourceRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                string strNewPath = Path.Combine(Settings.NewDirectory, Request.ProjectSourceName);
                if (File.Exists(strNewPath)) {
                    objWsItem.Project.SourcePath = strNewPath;
                    return new GetStatusResponse(Request, new List<Data.WorkspaceItem>() { objWsItem.GetWorkspaceItem() });
                }
                return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "File not found"));
            }
            return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}