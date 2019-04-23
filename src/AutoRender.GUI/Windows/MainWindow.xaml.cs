using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoRender.Messaging.Response;
using AutoRender.Data;
using Mitto.IMessaging;
using Mitto.Messaging.Response;
using AutoRender.Messaging.Request;
using AutoRender.Subscription.Messaging.Action.Request;
using AutoRender.Subscription.Messaging;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow {
        private MainViewModel _objViewModel = new MainViewModel();
        private Connection _objConnection;

        public MainWindow() : base() {
            this.DataContext = _objViewModel;
            InitializeComponent();

            base.StatusChanged += delegate (WindowStatus pStatus, string pMessage) {
                _objViewModel.SetStatus(pStatus, pMessage);
            };
            Connect();
        }

        private void Connect() {
            SetLoading("Loading Projects...");
            _objConnection = new Connection();
            _objConnection.Ready += _objConnection_Ready;
            _objConnection.Disconnected += _objConnection_Disconnected;
            _objConnection.Start();

            SendWorkspaceUpdatedRequestAction.WorkspaceUpdated += SendWorkspaceUpdatedRequestAction_WorkspaceUpdated;
        }

        private void SendWorkspaceUpdatedRequestAction_WorkspaceUpdated(IClient pClient, List<WorkspaceUpdatedEventArgs> pUpdates) {
            pUpdates.ForEach(u => {
                switch (u.Action) {
                    case WorkspaceAction.New:
                    case WorkspaceAction.Updated:
                        _objViewModel.Update(u.WorkspaceItem);
                        break;

                    case WorkspaceAction.Deleted:
                        _objViewModel.Delete(u.WorkspaceItem);
                        break;
                }
            });
        }

        /// <summary>
        /// When the connection has been set up, subscribe and get the status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _objConnection_Ready(object sender, EventArgs e) {
            Reload();
        }

        private void _objConnection_Disconnected(object sender, EventArgs e) {
            this.SetLoading("Connection lost, reconnecting...");
        }

        private void UpdateStatus() {
            Task.Run(() => {
                var blnSuccess = false;
                while (!blnSuccess && _objConnection.IsConnected) {
                    ManualResetEvent objBlock = new ManualResetEvent(false);
                    _objConnection.Request<GetStatusResponse>(new GetStatusRequest(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            blnSuccess = true;
                            _objViewModel.Clear();
                            _objViewModel.Update(r.WorkspaceItems);
                        } else {
                            Thread.Sleep(2000);
                        }
                        objBlock.Set();
                    });
                    objBlock.WaitOne();
                }
            });
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            var lstWsItems = _objViewModel.WorkspaceItems.Where(i => i.SelectedForHandling);
            if (lstWsItems.Count() > 0) {
                foreach (var objWsItem in lstWsItems) {
                    _objConnection.Request<ACKResponse>(new JobStartRequest(objWsItem.ID), (r) => {
                        //deselect when we were able to queue the job
                        if (r.Status.State == ResponseState.Success) {
                            objWsItem.SelectedForHandling = false;
                        }
                    });
                }
            }
        }

        private void TargetNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.TargetName)) {
                objViewModel.IsUpdating = true;

                _objConnection.Request<ACKResponse>(new UpdateProjectTargetRequest(objViewModel.ID, objViewModel.TargetName), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
                        objViewModel.IsUpdating = false;
                    } else {
                        _objConnection.Request<GetStatusResponse>(new GetStatusRequest(objViewModel.ID.ToString()), (s) => {
                            if (s.Status.State == ResponseState.Success) {
                                s.WorkspaceItems.ForEach(i => objViewModel.Update(i));
                            } else {
                                MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
                            }
                            objViewModel.IsUpdating = false;
                        });
                    }
                });
            }
        }

        private void SourceNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as System.Windows.Controls.Button).BindingGroup.Owner as System.Windows.Controls.DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!String.IsNullOrEmpty(objViewModel.SourceName)) {
                objViewModel.IsUpdating = true;

                _objConnection.Request<ACKResponse>(new UpdateProjectSourceRequest(objViewModel.ID, objViewModel.SourceName), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
                        objViewModel.IsUpdating = false;
                    } else {
                        _objConnection.Request<GetStatusResponse>(new GetStatusRequest(objViewModel.ID.ToString()), (s) => {
                            if (s.Status.State == ResponseState.Success) {
                                s.WorkspaceItems.ForEach(i => _objViewModel.Update(i));
                            } else {
                                MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
                            }
                            objViewModel.IsUpdating = false;
                        });
                    }
                });
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            UpdateStatus();
            SetLoading("Reloading server data...");

            _objConnection.Request<GetStatusResponse>(new ReloadRequest(), (r) => {
                if (r.Status.State == ResponseState.Success) {
                    _objViewModel.Update(r.WorkspaceItems);
                } else {
                    MessageBox.Show("Failed reloading item, please try again or contact Nico the almighty");
                }
                EndLoading();
            });
        }

        private void Reload() {
            Task.Run(() => {
                SetLoading("Connection ready, fetching status from server...");
                UpdateStatus();

                var blnSuccess = false;
                SetLoading("Subscribing to workspace update event");
                do {
                    ManualResetEvent objBlock = new ManualResetEvent(false);
                    _objConnection.Request<ACKResponse>(new WorkspaceUpdatedSubscribe(), (r) => {
                        if (r.Status.State == ResponseState.Success) {
                            blnSuccess = true;
                        } else {
                            Thread.Sleep(2000);
                        }
                    });
                    objBlock.WaitOne();
                } while (!blnSuccess && _objConnection.IsConnected);
                EndLoading();
            });
        }

        private void mnuStart_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Processable || objWorkspaceItemViewModel.Status == Status.Paused) {
                objWorkspaceItemViewModel.IsUpdating = true;
                objWorkspaceItemViewModel.SelectedForHandling = true;

                _objConnection.Request<ACKResponse>(new JobStartRequest(objWorkspaceItemViewModel.ID), (r) => {
                    //deselect when we were able to queue the job
                    if (r.Status.State == ResponseState.Success) {
                        objWorkspaceItemViewModel.SelectedForHandling = false;
                    } else {
                        MessageBox.Show("Failed starting item, please try again or contact Nico the almighty");
                    }
                    objWorkspaceItemViewModel.IsUpdating = false;
                });
            }
        }

        private void mnuStop_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
                objWorkspaceItemViewModel.IsUpdating = true;
                objWorkspaceItemViewModel.SelectedForHandling = false;

                _objConnection.Request<ACKResponse>(new JobStopRequest(objWorkspaceItemViewModel.ID), (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        objWorkspaceItemViewModel.SelectedForHandling = true;
                    } else {
                        MessageBox.Show("Failed stopping item, please try again or contact Nico the almighty");
                    }
                    objWorkspaceItemViewModel.IsUpdating = false;
                });
            }
        }

        private void mnuPause_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
                objWorkspaceItemViewModel.IsUpdating = true;

                _objConnection.Request<ACKResponse>(new JobPauseRequest(objWorkspaceItemViewModel.ID), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed pausing item, please try again or contact Nico the almighty");
                    }
                    objWorkspaceItemViewModel.IsUpdating = false;
                });
            }
        }

        private void mnuEditTargetName_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            objWorkspaceItemViewModel.TargetNameIsEditing = true;
        }

        private void mnuEditSourceName_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            objWorkspaceItemViewModel.SourceNameIsEditing = true;
        }

        private void ProjectField_CreateProjectClicked(object sender, RoutedEventArgs e) {
            try {
                if (!File.Exists(Settings.ShotcutExecutable)) { throw new Exception("Shotcut executable not found"); }

                var objWorkspaceItemViewModel = (sender as System.Windows.Documents.Hyperlink).DataContext as WorkspaceItemViewModel;
                Task.Run(() => {
                    if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.ProjectMissing) {
                        objWorkspaceItemViewModel.IsUpdating = true;
                        try {
                            FileInfo objFI = MeltConfig.CreateConfig(objWorkspaceItemViewModel.WorkspaceItem);
                            new Process {
                                StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + objFI.FullName + "\"")
                            }.Start();
                        } catch (Exception ex) {
                            Console.WriteLine(ex);
                        }
                        objWorkspaceItemViewModel.IsUpdating = false;
                    }
                });
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuOpenShotcut_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            string strPath = Path.Combine(Settings.ProjectPath, objWorkspaceItemViewModel.ProjectName);
            if (File.Exists(strPath) && File.Exists(Settings.ShotcutExecutable)) {
                new Process {
                    StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + strPath + "\"")
                }.Start();
            }
        }
    }
}