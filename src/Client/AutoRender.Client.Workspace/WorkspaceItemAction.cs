using System;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;

namespace AutoRender {

    public class WorkspaceItemAction {
        private readonly Client.Connection.Client Client;

        public WorkspaceItemAction(Client.Connection.Client pConnection) {
            Client = pConnection;
        }

        public void ChangeTargetName(Guid pItemID, string pName, Action<GetStatusResponse> pAction) {
            Client.Request(new UpdateProjectTargetRequest(pItemID, pName), pAction);
        }

        public void ChangeSourceName(Guid pItemID, string pName, Action<GetStatusResponse> pAction) {
            Client.Request(new UpdateProjectSourceRequest(pItemID, pName), pAction);
        }

        public void StartJob(Guid pItemID, Action<GetStatusResponse> pAction) {
            Client.Request(new JobStartRequest(pItemID), pAction);
        }

        public void StopJob(Guid pItemID, Action<GetStatusResponse> pAction) {
            Client.Request(new JobStopRequest(pItemID), pAction);
        }

        public void PauseJob(Guid pItemID, Action<GetStatusResponse> pAction) {
            Client.Request(new JobPauseRequest(pItemID), pAction);
        }
    }
}