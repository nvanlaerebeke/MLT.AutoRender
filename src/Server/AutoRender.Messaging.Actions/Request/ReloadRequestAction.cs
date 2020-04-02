using System.Linq;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using Mitto.IMessaging;
using Mitto.Messaging.Action;

namespace AutoRender.Messaging.Action.Request {

    public class ReloadRequestAction : RequestAction<ReloadRequest, GetStatusResponse> {

        public ReloadRequestAction(IClient pClient, ReloadRequest pRequest) : base(pClient, pRequest) {
        }

        public override GetStatusResponse Start() {
            Workspace.WorkspaceFactory.Get().Reload();

            return new GetStatusResponse(
                Request,
                Workspace.WorkspaceFactory.Get().GetAll().Select(i => i.GetWorkspaceItem()).ToList()
            );
        }
    }
}