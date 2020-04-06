using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoRender.Client.Connection;
using AutoRender.Data;

namespace AutoRender.Client.Runtime {

    public class WorkspaceConnection {

        public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

        public event EventHandler RefreshRequired;

        public event EventHandler<List<WorkspaceUpdatedEventArgs>> WorkspaceUpdated;

        private readonly ConnectionManager ConnectionManager;
        public readonly Workspace.Workspace Workspace;

        public WorkspaceConnection(ConnectionManager pManager, Workspace.Workspace pWorkspace) {
            ConnectionManager = pManager;
            Workspace = pWorkspace;

            ConnectionManager.StatusChanged += ConnectionManager_StatusChanged;
            Workspace.WorkspaceUpdated += Workspace_WorkspaceUpdated;
            Workspace.RefreshRequired += Workspace_RefreshRequired;
        }

        private void Workspace_RefreshRequired(object sender, EventArgs e) {
            RefreshRequired?.Invoke(sender, e);
        }

        public ConnectionStatus Status {
            get {
                return ConnectionManager.Status;
            }
        }

        public void Start() {
            ConnectionManager.Start();
        }

        private void Workspace_WorkspaceUpdated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            WorkspaceUpdated?.Invoke(this, e);
        }

        private void ConnectionManager_StatusChanged(object sender, ConnectionStatus e) {
            switch (e) {
                case ConnectionStatus.Connected:
                    _ = Task.Run(() => Subscribe());
                    break;
            }
            ConnectionStatusChanged?.Invoke(sender, e);
        }

        private void Subscribe() {
            if (ConnectionManager.Status == ConnectionStatus.Connected) {
                if (!Workspace.Subscribe()) {
                    Task.Delay(5000);
                    Subscribe();
                }
            }
        }

        /// <summary>
        /// ToDo Below
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Connection_Ready(object sender, EventArgs e) {
        //Load();
        //}

        /*public void Refesh() {
            Load();
        }*/

        /* private void Load() {
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
         }*/

        /*private async Task<bool> Subscribe() {
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
        }*/

        /*internal async Task<List<WorkspaceItem>> ReLoad() {
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

        public async Task<bool> GetStatus() {
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
        }*/
    }
}