﻿using AutoRender.Messaging.Request;
using Mitto.Messaging.Response;
using System;

namespace AutoRender {

    internal class WorkspaceItemAction {
        private readonly Connection Connection;
        private readonly Action<bool> Action;
        private readonly Action<ACKResponse> Callback;
        private readonly Guid ProjectID;

        public WorkspaceItemAction(Connection pConnection, Guid pProjectID, Action<bool> pAction) {
            Connection = pConnection;
            ProjectID = pProjectID;
            Action = pAction;
            Callback = (r) => { Action.Invoke(r.Status.State == Mitto.IMessaging.ResponseState.Success); };
        }

        public void ChangeTargetName(string pName) {
            try {
                Connection.Request(new UpdateProjectTargetRequest(ProjectID, pName), Callback);
            } catch (Exception) {
                Action.Invoke(false);
            }
        }

        public void ChangeSourceName(string pName) {
            try {
                Connection.Request(new UpdateProjectSourceRequest(ProjectID, pName), Callback);
            } catch (Exception) {
                Action.Invoke(false);
            }
        }

        public void StartJob() {
            try {
                Connection.Request(new JobStartRequest(ProjectID), Callback);
            } catch (Exception) {
                Action.Invoke(false);
            }
        }

        public void StopJob() {
            try {
                Connection.Request(new JobStopRequest(ProjectID), Callback);
            } catch (Exception) {
                Action.Invoke(false);
            }
        }

        public void PauseJob() {
            try {
                Connection.Request(new JobPauseRequest(ProjectID), Callback);
            } catch (Exception) {
                Action.Invoke(false);
            }
        }
    }
}