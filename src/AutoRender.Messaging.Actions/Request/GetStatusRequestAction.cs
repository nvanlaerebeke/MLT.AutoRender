using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;
using System;
using System.Collections.Generic;

namespace AutoRender.Messaging.Action.Request {

    public class GetStatusRequestAction : RequestAction<GetStatusRequest, GetStatusResponse> {

        public GetStatusRequestAction(IClient pClient, GetStatusRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            List<WorkspaceItem> lstItems = new List<WorkspaceItem>();
            try {
                foreach (var objKvp in Workspace.WorkspaceFactory.Get().WorkspaceItems) {
                    lstItems.Add(objKvp.Value.GetWorkspaceItem());
                }
                return new GetStatusResponse(Request, lstItems);
            } catch(Exception ex) {
                return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, ex.Message));
            }
        }
    }
}