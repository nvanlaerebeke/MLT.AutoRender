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

        public void Start() {
            Connection.Start();
        }

        public void Refesh() {
            Load();
        }

        #region EventHandlers

        private void Connection_Ready(object sender, EventArgs e) {
            Load();
        }

        private void Connection_Disconnected(object sender, EventArgs e) {
            StatusChanged?.Invoke(this, "Connecting");
        }

        #endregion EventHandlers

        #region Private Methods

        private void Load() {
            Task.Run(() => {
                while (!Setup()) {
                    if (Connection.IsConnected) {
                        Thread.Sleep(2000);
                    } else {
                        StatusChanged?.Invoke(this, "Connecting");
                        return;
                    }
                }
            });
        }

        private bool Setup() {
            StatusChanged?.Invoke(this, "Loading");

            var objSubTask = Subscribe();
            var objGetStatusTask = GetStatus();
            objSubTask.Wait();
            objGetStatusTask.Wait();

            if (objSubTask.Result && objGetStatusTask.Result) {
                StatusChanged?.Invoke(this, "Ready");
            }
            return objSubTask.Result && objGetStatusTask.Result;
        }

        private async Task<bool> Subscribe() {
            return await Task.Run(() => {
                var blnSuccess = false;
                do {
                    var objBlock = new ManualResetEvent(false);
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

        internal async Task<List<WorkspaceItem>> ReLoad() {
            return await Task.Run(() => {
                var lstItems = new List<WorkspaceItem>();
                var blnSuccess = false;
                do {
                    var objBlock = new ManualResetEvent(false);
                    Connection.Request<GetStatusResponse>(new ReloadRequest(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            lstItems.AddRange(r.WorkspaceItems);
                            blnSuccess = true;
                        } else {
                            Thread.Sleep(2000);
                        }
                        _ = objBlock.Set();
                    });
                    _ = objBlock.WaitOne();
                } while (!blnSuccess && Connection.IsConnected);
                return lstItems;
            });
        }

        private async Task<bool> GetStatus() {
            return await Task.Run(() => {
                var blnSuccess = false;
                do {
                    var objBlock = new ManualResetEvent(false);
                    Connection.Request<GetStatusResponse>(new GetStatusRequest(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            blnSuccess = true;
                            WorkspaceUpdated?.Invoke(this, r.WorkspaceItems);
                        } else {
                            Thread.Sleep(2000);
                        }
                        _ = objBlock.Set();
                    });
                    _ = objBlock.WaitOne();
                } while (!blnSuccess && Connection.IsConnected);
                return blnSuccess;
            });
        }

        #endregion Private Methods
    }
}