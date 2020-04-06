using System.Collections.Generic;
using System.IO;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Server.Config;
using AutoRender.Video;
using AutoRender.Workspace;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class UpdateProjectSourceRequestAction : RequestAction<UpdateProjectSourceRequest, GetStatusResponse> {

        public UpdateProjectSourceRequestAction(IClient pClient, UpdateProjectSourceRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            var objWsItem = WorkspaceFactory.Get().Get(Request.ItemID);
            if (objWsItem != null && objWsItem.Project != null) {
                if (objWsItem.UpdateNew(new VideoInfoProvider().Get(Path.Combine(Settings.NewDirectory, Request.ProjectSourceName)))) {
                    return new GetStatusResponse(Request, new List<Data.WorkspaceItem>() { objWsItem.GetWorkspaceItem() });
                }
                return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "File not found"));
            }
            return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, "Project not found"));
        }
    }
}