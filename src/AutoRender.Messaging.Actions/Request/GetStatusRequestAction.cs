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
                Workspace.WorkspaceFactory.Get().WorkspaceItems.ForEach(i =>
                    lstItems.Add(new WorkspaceItem() {
                        ID = i.ID,
                        Project = i.Project.GetProject(),
                        New = i.New,
                        Final = i.Final
                    })
                );
                return new GetStatusResponse(Request, lstItems);
            } catch(Exception ex) {
                return new GetStatusResponse(Request, new ResponseStatus(ResponseState.Error, ex.Message));
            }
        }
    }
}