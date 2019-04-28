using AutoRender.Data;
using AutoRender.Messaging.Request;
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

    public class UpdateProjectSourceRequestAction : RequestAction<UpdateProjectSourceRequest, ACKResponse> {

        public UpdateProjectSourceRequestAction(IClient pClient, UpdateProjectSourceRequest pRequest) : base(pClient, pRequest) {
        }

        public override ACKResponse Start() {
            var objItem = WorkspaceFactory.Get().Get(Request.ProjectID);
            if (objItem != null) {
                string strNewPath = Path.Combine(Settings.NewDirectory, Request.ProjectSourceName);
                if (File.Exists(strNewPath)) {
                    objItem.Project.SourcePath = strNewPath;

                    new SubscriptionClient<WorkspaceUpdatedHandler>(Client).Notify(new SendWorkspaceUpdatedRequest(
                        new List<WorkspaceUpdatedEventArgs>() {
                            new WorkspaceUpdatedEventArgs(
                                objItem.GetWorkspaceItem(),
                                WorkspaceAction.Updated
                            )
                        }
                    ));

                    return new ACKResponse(Request);
                }
                return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"File {strNewPath} not found"));
            }
            return new ACKResponse(Request, new ResponseStatus(ResponseState.Error, $"Project {Request.ProjectID} not found"));
        }
    }
}