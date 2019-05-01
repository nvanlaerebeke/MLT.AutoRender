using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using System;
using System.Collections.Generic;

namespace AutoRender {

    internal class WorkspaceItemAction {
        private readonly Connection Connection;
        private readonly Action<bool, List<WorkspaceItem>> Action;
        private readonly Action<GetStatusResponse> Callback;
        private readonly Guid ItemID;

        public WorkspaceItemAction(Connection pConnection, Guid pItemID, Action<bool, List<WorkspaceItem>> pAction) {
            Connection = pConnection;
            ItemID = pItemID;
            Action = pAction;
            Callback = (r) => {
                Action.Invoke(r.Status.State == Mitto.IMessaging.ResponseState.Success, r.WorkspaceItems);
            };
        }

        public void ChangeTargetName(string pName) {
            try {
                Connection.Request(new UpdateProjectTargetRequest(ItemID, pName), Callback);
            } catch (Exception) {
                Action.Invoke(false, null);
            }
        }

        public void ChangeSourceName(string pName) {
            try {
                Connection.Request(new UpdateProjectSourceRequest(ItemID, pName), Callback);
            } catch (Exception) {
                Action.Invoke(false, null);
            }
        }

        public void StartJob() {
            try {
                Connection.Request(new JobStartRequest(ItemID), Callback);
            } catch (Exception) {
                Action.Invoke(false, null);
            }
        }

        public void StopJob() {
            try {
                Connection.Request(new JobStopRequest(ItemID), Callback);
            } catch (Exception) {
                Action.Invoke(false, null);
            }
        }

        public void PauseJob() {
            try {
                Connection.Request(new JobPauseRequest(ItemID), Callback);
            } catch (Exception) {
                Action.Invoke(false, null);
            }
        }
    }
}