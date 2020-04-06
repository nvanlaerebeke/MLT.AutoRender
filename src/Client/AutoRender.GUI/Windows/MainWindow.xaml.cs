using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoRender.Client.Config;
using AutoRender.Client.Runtime;
using AutoRender.Data;
using Mitto.IMessaging;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow {
        private readonly MainViewModel _objViewModel = new MainViewModel();
        private readonly WorkspaceConnection Connection;

        public MainWindow(WorkspaceConnection pConnection) : base() {
            Connection = pConnection;

            DataContext = _objViewModel;
            InitializeComponent();

            Connection.ConnectionStatusChanged += Connection_ConnectionStatusChanged;
            Connection.WorkspaceUpdated += Connection_WorkspaceUpdated;
            Connection.RefreshRequired += Connection_RefreshRequired;

            StatusChanged += delegate (WindowStatus pStatus, string pMessage) {
                _objViewModel.SetStatus(pStatus, pMessage);
            };

            Load();
        }

        private void Connection_RefreshRequired(object sender, EventArgs e) {
            Load();
        }

        private void Connection_WorkspaceUpdated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            e.ForEach(u => {
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

        private void Connection_ConnectionStatusChanged(object sender, Client.Connection.ConnectionStatus e) {
            if (e == Client.Connection.ConnectionStatus.Disconnected) {
                SetLoading("Connecting...");
            } else if (e == Client.Connection.ConnectionStatus.Connected) {
                Load();
            }
        }

        private void Load() {
            if (Connection.Status == Client.Connection.ConnectionStatus.Connected) {
                SetLoading("Refreshing...");
                Connection.Workspace.GetStatus((r) => {
                    _objViewModel.Clear();
                    _objViewModel.Update(r.WorkspaceItems);
                    EndLoading();
                });
            } else {
                SetLoading("Connecting...");
            }
        }

        #region Server Actions

        #region Global

        private void ReLoad_Click(object sender, RoutedEventArgs e) {
            Reload();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            Refresh();
        }

        private void StartAll_Click(object sender, RoutedEventArgs e) {
            _objViewModel.WorkspaceItems.Where(i => i.SelectedForHandling).ToList().ForEach(i => {
                i.IsUpdating = true;
                Connection.Workspace.WorkspaceItem.StartJob(i.ID, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        _ = MessageBox.Show($"Failed starting {i.ProjectName}, please try again or contact Nico the almighty");
                    }
                    i.SelectedForHandling = false;
                    i.IsUpdating = false;
                });
            });
        }

        #endregion Global

        #region WorkspaceItem Actions

        private void TargetNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.TargetName)) {
                objViewModel.IsUpdating = true;
                Connection.Workspace.WorkspaceItem.ChangeTargetName(objViewModel.ID, objViewModel.TargetName, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        _ = MessageBox.Show($"Failed target name to {objViewModel.TargetName}, please try again or contact Nico the almighty");
                    }
                    objViewModel.IsUpdating = false;
                });
            }
        }

        private void SourceNameChanged(object sender, RoutedEventArgs e) {
            var objViewModel = ((sender as Button).BindingGroup.Owner as DataGridRow).DataContext as WorkspaceItemViewModel;
            if (!string.IsNullOrEmpty(objViewModel.SourceName)) {
                objViewModel.IsUpdating = true;
                Connection.Workspace.WorkspaceItem.ChangeSourceName(objViewModel.ID, objViewModel.SourceName, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        MessageBox.Show($"Failed updating name to {objViewModel.SourceName}, please try again or contact Nico the almighty");
                    }
                    objViewModel.IsUpdating = false;
                });
            }
        }

        #endregion WorkspaceItem Actions

        #region ContextMenu Actions

        private void Start_Click(object sender, RoutedEventArgs e) {
            var objViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (
                (objViewModel != null && objViewModel.Status == Status.Processable) ||
                objViewModel.Status == Status.Paused
            ) {
                objViewModel.IsUpdating = true;
                objViewModel.SelectedForHandling = true;

                Connection.Workspace.WorkspaceItem.StartJob(objViewModel.ID, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        _ = MessageBox.Show($"Failed to start {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    }
                    objViewModel.IsUpdating = false;
                    objViewModel.SelectedForHandling = false;
                });
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            if (
                (sender as MenuItem).DataContext is WorkspaceItemViewModel objViewModel &&
                objViewModel.Status == Status.Busy
            ) {
                objViewModel.IsUpdating = true;
                objViewModel.SelectedForHandling = false;

                Connection.Workspace.WorkspaceItem.StopJob(objViewModel.ID, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        MessageBox.Show($"Failed to stop {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    }
                    objViewModel.SelectedForHandling = true;
                    objViewModel.IsUpdating = false;
                });
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e) {
            var objViewModel = (sender as MenuItem).DataContext as WorkspaceItemViewModel;
            if (objViewModel != null && objViewModel.Status == Status.Busy) {
                objViewModel.IsUpdating = true;
                Connection.Workspace.WorkspaceItem.PauseJob(objViewModel.ID, (r) => {
                    if (r.Status.State == ResponseState.Success) {
                        _objViewModel.Update(r.WorkspaceItems);
                    } else {
                        MessageBox.Show($"Failed to pause {objViewModel.ProjectName}, please try again or contact Nico the almighty");
                    }
                    objViewModel.IsUpdating = false;
                });
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
                    try {
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
                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message); ;
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

        private void ReloadRequestAction_ReloadRequested(object sender, EventArgs e) {
            Refresh();
        }

        private void Reload() {
            if (_objViewModel.WorkspaceItems.Any(i => i.Status == Status.Busy)) {
                var objResult = MessageBox.Show("Reloading will stop all renders, are you sure you wish to continue?", "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (objResult != MessageBoxResult.Yes) {
                    return;
                }
            }
            SetLoading("ReLoading Workspace...");
            Connection.Workspace.Reload((r) => {
                _objViewModel.Clear();
                if (r.Status.State == ResponseState.Success) {
                    _objViewModel.Update(r.WorkspaceItems);
                } else {
                    _ = MessageBox.Show($"Failed to reload the workspace, please try again or contact Nico the almighty");
                }
                EndLoading();
            });
        }

        private void Refresh() {
            SetLoading("Refreshing Workspace...");
            Connection.Workspace.GetStatus((r) => {
                _objViewModel.Clear();
                if (r.Status.State == ResponseState.Success) {
                    _objViewModel.Update(r.WorkspaceItems);
                } else {
                    _ = MessageBox.Show($"Failed to refresh the workspace, please try again or contact Nico the almighty");
                }
                EndLoading();
            });
        }

        private void Settings_Click(object sender, RoutedEventArgs e) {
            SetLoading("Editing Settings...");
            var objSettingsWindow = new SettingsWindow(Connection) { Owner = this };
            objSettingsWindow.Closed += delegate (object s, EventArgs args) {
                EndLoading();
            };
            objSettingsWindow.Show();
        }
    }
}