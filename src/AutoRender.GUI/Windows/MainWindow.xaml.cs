using AutoRender.Data;
using AutoRender.Messaging.Request;
using AutoRender.Messaging.Response;
using AutoRender.Subscription.Messaging;
using AutoRender.Subscription.Messaging.Action.Request;
using Mitto.IMessaging;
using Mitto.Messaging.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow {
        private readonly MainViewModel _objViewModel = new MainViewModel();
        private readonly ConnectionManager ConnectionManager = new ConnectionManager();

        public MainWindow() : base() {
            DataContext = _objViewModel;
            InitializeComponent();

            StatusChanged += delegate (WindowStatus pStatus, string pMessage) {
                _objViewModel.SetStatus(pStatus, pMessage);
            };

            ConnectionManager.StatusChanged += ConnectionManager_StatusChanged;
            ConnectionManager.WorkspaceUpdated += ConnectionManager_WorkspaceUpdated;
            ConnectionManager.Connect();

            SendWorkspaceUpdatedRequestAction.WorkspaceUpdated += SendWorkspaceUpdatedRequestAction_WorkspaceUpdated;
        }

        private void ConnectionManager_WorkspaceUpdated(object sender, List<WorkspaceItem> pWorkspaceItems) {
            if (pWorkspaceItems != null) {
                _objViewModel.Clear();
                _objViewModel.Update(pWorkspaceItems);
            }
        }

        private void ConnectionManager_StatusChanged(object sender, string e) {
            switch (e) {
                case "Connecting":
                    SetLoading("Connecting...");
                    break;

                case "Loading":
                    SetLoading("Loading...");
                    break;

                case "Ready":
                    EndLoading();
                    break;
            }
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

        private void TargetNameChanged(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.TargetName)) {
                objViewModel.IsUpdating = true;

                ConnectionManager.Connection.Request<ACKResponse>(new UpdateProjectTargetRequest(objViewModel.ID, objViewModel.TargetName), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
                        objViewModel.IsUpdating = false;
                    } else {
                        ConnectionManager.Connection.Request<GetStatusResponse>(new GetStatusRequest(objViewModel.ID.ToString()), (s) => {
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
            WorkspaceItemViewModel objViewModel = ((sender as System.Windows.Controls.Button).BindingGroup.Owner as System.Windows.Controls.DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.SourceName)) {
                objViewModel.IsUpdating = true;

                ConnectionManager.Connection.Request<ACKResponse>(new UpdateProjectSourceRequest(objViewModel.ID, objViewModel.SourceName), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed updating name, please try again or contact Nico the almighty");
                        objViewModel.IsUpdating = false;
                    } else {
                        ConnectionManager.Connection.Request<GetStatusResponse>(new GetStatusRequest(objViewModel.ID.ToString()), (s) => {
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

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            ConnectionManager.Reload();
        }

        private void Start_Click(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Processable || objWorkspaceItemViewModel.Status == Status.Paused) {
                objWorkspaceItemViewModel.IsUpdating = true;
                objWorkspaceItemViewModel.SelectedForHandling = true;

                ConnectionManager.Connection.Request<ACKResponse>(new JobStartRequest(objWorkspaceItemViewModel.ID), (r) => {
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

        private void Stop_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
                objWorkspaceItemViewModel.IsUpdating = true;
                objWorkspaceItemViewModel.SelectedForHandling = false;

                ConnectionManager.Connection.Request<ACKResponse>(new JobStopRequest(objWorkspaceItemViewModel.ID), (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        objWorkspaceItemViewModel.SelectedForHandling = true;
                    } else {
                        MessageBox.Show("Failed stopping item, please try again or contact Nico the almighty");
                    }
                    objWorkspaceItemViewModel.IsUpdating = false;
                });
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objWorkspaceItemViewModel != null && objWorkspaceItemViewModel.Status == Status.Busy) {
                objWorkspaceItemViewModel.IsUpdating = true;

                ConnectionManager.Connection.Request<ACKResponse>(new JobPauseRequest(objWorkspaceItemViewModel.ID), (r) => {
                    if (r.Status.State != ResponseState.Success) {
                        MessageBox.Show("Failed pausing item, please try again or contact Nico the almighty");
                    }
                    objWorkspaceItemViewModel.IsUpdating = false;
                });
            }
        }

        private void EditTargetName_Click(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            objWorkspaceItemViewModel.TargetNameIsEditing = true;
        }

        private void EditSourceName_Click(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            objWorkspaceItemViewModel.SourceNameIsEditing = true;
        }

        private void ProjectField_CreateProjectClicked(object sender, RoutedEventArgs e) {
            try {
                if (!File.Exists(Settings.ShotcutExecutable)) { throw new Exception("Shot-cut executable not found"); }

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

        private void OpenShotcut_Click(object sender, RoutedEventArgs e) {
            WorkspaceItemViewModel objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            string strPath = Path.Combine(Settings.ProjectPath, objWorkspaceItemViewModel.ProjectName);
            if (File.Exists(strPath) && File.Exists(Settings.ShotcutExecutable)) {
                new Process {
                    StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + strPath + "\"")
                }.Start();
            }
        }
    }
}