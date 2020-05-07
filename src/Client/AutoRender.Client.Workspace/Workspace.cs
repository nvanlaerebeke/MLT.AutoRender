using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Subscription.Messaging;
using Mitto.IMessaging;
using Mitto.Messaging.Response;

namespace AutoRender.Client.Workspace {

    public class Workspace {
        private readonly Connection.Client _objClient;

        public event EventHandler<List<WorkspaceUpdatedEventArgs>> WorkspaceUpdated;

        public event EventHandler RefreshRequired;

        public readonly WorkspaceItemAction WorkspaceItem;

        public Workspace(Connection.Client pClient) {
            _objClient = pClient;
            WorkspaceItem = new WorkspaceItemAction(pClient);

            //ToDo: Dependency should be the other way around, actions should start 'something'
            AutoRender.Subscription.Messaging.Action.Request.SendWorkspaceUpdatedRequestAction.WorkspaceUpdated += SendWorkspaceUpdatedRequestAction_WorkspaceUpdated;
            AutoRender.Messaging.Actions.Request.ReloadRequestAction.ReloadRequested += ReloadRequestAction_ReloadRequested; ;
        }

        private void ReloadRequestAction_ReloadRequested(object sender, EventArgs e) {
            RefreshRequired?.Invoke(this, e);
        }

        private void SendWorkspaceUpdatedRequestAction_WorkspaceUpdated(IClient pClient, List<WorkspaceUpdatedEventArgs> pUpdates) {
            WorkspaceUpdated?.Invoke(pClient, pUpdates);
        }

        public bool Subscribe() {
            var res = _objClient.Request<ACKResponse>(new WorkspaceUpdatedSubscribe());
            res.Wait(5000);
            if (res.IsCompleted && res.Status == TaskStatus.RanToCompletion) {
                return res.Result.Status.State == ResponseState.Success;
            }
            return false;
        }

        public void Reload(Action<GetStatusResponse> pAction) {
            _objClient.Request(new ReloadRequest(), pAction);
        }

        public void GetStatus(Action<GetStatusResponse> pAction) {
            _objClient.Request(new GetStatusRequest(), pAction);
        }

        public void GetSettings(Action<GetSettingsResponse> pAction) {
            _objClient.Request(new GetSettingsRequest(), pAction);
        }

        public void UpdateSettings(ServerSettings pSettings, Action<ACKResponse> pAction) {
            _objClient.Request(new UpdateSettingsRequest(pSettings), pAction);
        }
    }
}