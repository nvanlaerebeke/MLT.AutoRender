using System;
using System.Linq;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class GetStatusRequestAction : RequestAction<GetStatusRequest, GetStatusResponse> {

        public GetStatusRequestAction(IClient pClient, GetStatusRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            try {
                return new GetStatusResponse(Request, Workspace.WorkspaceFactory.Get().GetAll().Select(i => i.GetWorkspaceItem()).ToList());
            } catch (Exception ex) {
                return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, ex.Message));
            }
        }
    }
}