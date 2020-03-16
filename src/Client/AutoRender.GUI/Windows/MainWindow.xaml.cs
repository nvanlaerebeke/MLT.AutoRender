using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoRender.Data;
using AutoRender.Subscription.Messaging.Action.Request;
using Mitto.IMessaging;

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
            ConnectionManager.Start();

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

        #region Server Actions

        #region Global

        private void ReLoad_Click(object sender, RoutedEventArgs e) {
            SetLoading("ReLoading Workspace...");
            _objViewModel.WorkspaceItems.Clear();
            Task.Run(() => {
                var objResult = ConnectionManager.ReLoad().Result;
                _objViewModel.Update(objResult);
                EndLoading();
            });
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            ConnectionManager.Refesh();
        }

        private void StartAll_Click(object sender, RoutedEventArgs e) {
            _objViewModel.WorkspaceItems.Where(i => i.SelectedForHandling).ToList().ForEach(i => {
                i.IsUpdating = true;
                new WorkspaceItemAction(ConnectionManager.Connection, i.ID, (r, wsis) => {
                    if (!r) {
                        MessageBox.Show($"Failed starting {i.ProjectName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                        i.SelectedForHandling = false;
                    }
                    i.IsUpdating = false;
                }).StartJob();
            });
        }

        #endregion Global

        #region WorkspaceItem Actions

        private void TargetNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.TargetName)) {
                objViewModel.IsUpdating = true;
                new WorkspaceItemAction(ConnectionManager.Connection, objViewModel.ID, (r, wsis) => {
                    if (!r) {
                        ConnectionManager.Refesh();
                        MessageBox.Show($"Failed target name to {objViewModel.TargetName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                    }
                    objViewModel.IsUpdating = false;
                }).ChangeTargetName(objViewModel.TargetName);
            }
        }

        private void SourceNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.SourceName)) {
                objViewModel.IsUpdating = true;
                new WorkspaceItemAction(ConnectionManager.Connection, objViewModel.ID, (r, wsis) => {
                    if (!r) {
                        ConnectionManager.Refesh();
                        MessageBox.Show($"Failed updating name to {objViewModel.SourceName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                    }
                    objViewModel.IsUpdating = false;
                }).ChangeSourceName(objViewModel.SourceName);
            }
        }

        #endregion WorkspaceItem Actions

        #region ContextMenu Actions

        private void Start_Click(object sender, RoutedEventArgs e) {
            var objViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objViewModel != null && objViewModel.Status == Status.Processable || objViewModel.Status == Status.Paused) {
                objViewModel.IsUpdating = true;
                objViewModel.SelectedForHandling = true;

                new WorkspaceItemAction(ConnectionManager.Connection, objViewModel.ID, (r, wsis) => {
                    if (!r) {
                        MessageBox.Show($"Failed to start {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                        objViewModel.SelectedForHandling = false;
                    }
                    objViewModel.IsUpdating = false;
                }).StartJob();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            var objViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objViewModel != null && objViewModel.Status == Status.Busy) {
                objViewModel.IsUpdating = true;
                objViewModel.SelectedForHandling = false;

                new WorkspaceItemAction(ConnectionManager.Connection, objViewModel.ID, (r, wsis) => {
                    if (!r) {
                        MessageBox.Show($"Failed to stop {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                        objViewModel.SelectedForHandling = true;
                    }
                    objViewModel.IsUpdating = false;
                }).StopJob();
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e) {
            var objViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objViewModel != null && objViewModel.Status == Status.Busy) {
                objViewModel.IsUpdating = true;
                new WorkspaceItemAction(ConnectionManager.Connection, objViewModel.ID, (r, wsis) => {
                    if (!r) {
                        MessageBox.Show($"Failed to pause {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    } else {
                        wsis.ForEach(_objViewModel.Update);
                    }
                    objViewModel.IsUpdating = false;
                }).PauseJob();
            }
        }

        #endregion ContextMenu Actions

        #endregion Server Actions

        private void EditTargetName_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            objWorkspaceItemViewModel.TargetNameIsEditing = true;
        }

        private void EditSourceName_Click(object sender, RoutedEventArgs e) {
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
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
                            var objFI = MeltConfig.CreateConfig(objWorkspaceItemViewModel.WorkspaceItem);
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
            var objWorkspaceItemViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            var strPath = Path.Combine(Settings.ProjectPath, objWorkspaceItemViewModel.ProjectName);
            if (File.Exists(strPath) && File.Exists(Settings.ShotcutExecutable)) {
                new Process {
                    StartInfo = new ProcessStartInfo(Settings.ShotcutExecutable, "\"" + strPath + "\"")
                }.Start();
            }
        }
    }
}