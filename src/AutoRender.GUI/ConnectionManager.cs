using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Subscription.Messaging;
using Mitto.IMessaging;
using Mitto.Messaging.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender {

    internal class ConnectionManager {
        public readonly Connection Connection;

        public event EventHandler<string> StatusChanged;

        public event EventHandler<List<WorkspaceItem>> WorkspaceUpdated;

        public ConnectionManager() {
            Connection = new Connection();
            Connection.Ready += Connection_Ready;
            Connection.Disconnected += Connection_Disconnected;
        }

        public void Connect() {
            StatusChanged?.Invoke(this, "Connecting");
            Connection.Start();
        }

        public void Reload() {
            Load();
        }

        #region EventHandlers

        private void Connection_Ready(object sender, EventArgs e) {
            Load();
        }

        private void Connection_Disconnected(object sender, EventArgs e) {
            Connect();
        }

        #endregion EventHandlers

        #region Private Methods

        private void Load() {
            Task.Run(() => {
                StatusChanged?.Invoke(this, "Loading");
                while (!Setup()) {
                    if (Connection.IsConnected) {
                        Setup();
                    } else {
                        Connect();
                    }
                }
                StatusChanged?.Invoke(this, "Ready");
            });
        }

        private bool Setup() {
            StatusChanged?.Invoke(this, "Loading");

            Task<bool> objSubTask = Subscribe();
            Task<bool> objGetStatusTask = GetStatus();

            objSubTask.Wait();
            objGetStatusTask.Wait();

            return (
                objSubTask.Result &&
                objGetStatusTask.Result
            );
        }

        private async Task<bool> Subscribe() {
            return await Task.Run(() => {
                bool blnSuccess = false;
                do {
                    ManualResetEvent objBlock = new ManualResetEvent(false);
                    Connection.Request<ACKResponse>(new WorkspaceUpdatedSubscribe(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            blnSuccess = true;
                        } else {
                            Thread.Sleep(2000);
                        }
                        objBlock.Set();
                    });
                    objBlock.WaitOne();
                } while (!blnSuccess && Connection.IsConnected);
                return blnSuccess;
            });
        }

        private async Task<bool> GetStatus() {
            return await Task.Run(() => {
                bool blnSuccess = false;
                do {
                    ManualResetEvent objBlock = new ManualResetEvent(false);
                    Connection.Request<GetStatusResponse>(new GetStatusRequest(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            blnSuccess = true;
                            WorkspaceUpdated?.Invoke(this, r.WorkspaceItems);
                        } else {
                            Thread.Sleep(2000);
                        }
                        objBlock.Set();
                    });
                    objBlock.WaitOne();
                } while (!blnSuccess && Connection.IsConnected);
                return blnSuccess;
            });
        }

        #endregion Private Methods
    }
}