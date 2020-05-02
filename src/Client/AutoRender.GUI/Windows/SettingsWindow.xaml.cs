using System.Windows.Forms;
using AutoRender.Client.Config;
using AutoRender.Client.Runtime;
using AutoRender.Data;
using Mitto.Messaging.Response;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : BaseWindow {
        private readonly WorkspaceConnection Connection;
        private readonly SettingsViewModel _objViewModel = new SettingsViewModel();

        public SettingsWindow(WorkspaceConnection pConnection) {
            Connection = pConnection;
            DataContext = _objViewModel;
            InitializeComponent();
            SetLoading("Getting settings...");

            _objViewModel.LocalProjectDirectory = Settings.ProjectPath;
            _objViewModel.ShotcutExecutable = Settings.ShotcutExecutable;
            _objViewModel.HostName = Settings.HostName;
            _objViewModel.Port = Settings.Port;
            _objViewModel.StorageLocation = Settings.StorageLocation;
            _objViewModel.BackupLocation = Settings.BackupLocation;

            Connection.Workspace.GetSettings((r) => {
                if (r.Status.State == Mitto.IMessaging.ResponseState.Success) {
                    _objViewModel.ServerFinalDirectory = r.FinalDirectory;
                    _objViewModel.ServerLogDirectory = r.LogDirectory;
                    _objViewModel.ServerMeltPath = r.MeltPath;
                    _objViewModel.ServerNewDirectory = r.NewDirectory;
                    _objViewModel.ServerProjectDirectory = r.ProjectDirectory;
                    _objViewModel.Threads = r.Threads;
                } else {
                    _ = MessageBox.Show($"Failed to get the settings, please try again or contact Nico the almighty");
                }
                EndLoading();
            });
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e) {
            SetLoading("Updating settings...");
            var blnRestartRequired = Settings.HostName != _objViewModel.HostName || Settings.Port != _objViewModel.Port;
            Connection.Workspace.UpdateSettings(
                new ServerSettings(
                    _objViewModel.ServerProjectDirectory,
                    _objViewModel.ServerNewDirectory,
                    _objViewModel.ServerFinalDirectory,
                    _objViewModel.ServerLogDirectory,
                    _objViewModel.ServerMeltPath,
                    _objViewModel.Threads
                ), (ACKResponse r) => {
                    EndLoading();
                    _ = uiFactory.StartNew(() => {
                        if (blnRestartRequired) {
                            Application.Restart();
                            System.Windows.Application.Current.Shutdown();
                        } else {
                            Close();
                        }
                    });
                });
            Settings.HostName = _objViewModel.HostName;
            Settings.Port = _objViewModel.Port;
            Settings.ProjectPath = _objViewModel.LocalProjectDirectory;
            Settings.ShotcutExecutable = _objViewModel.ShotcutExecutable;
            Settings.StorageLocation = _objViewModel.StorageLocation;
            Settings.BackupLocation = _objViewModel.BackupLocation;
        }

        private void btnBrowseProjectDirectory_Click(object sender, System.Windows.RoutedEventArgs e) {
            var objDialog = new FolderBrowserDialog {
                SelectedPath = _objViewModel.LocalProjectDirectory
            };
            var res = objDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK) {
                _objViewModel.LocalProjectDirectory = objDialog.SelectedPath;
            }
        }

        private void btnBrowseShotcutExecutible_Click(object sender, System.Windows.RoutedEventArgs e) {
            var objDialog = new Microsoft.Win32.OpenFileDialog {
                InitialDirectory = System.IO.Path.GetDirectoryName(_objViewModel.ShotcutExecutable)
            };
            if (objDialog.ShowDialog() == true) {
                _objViewModel.ShotcutExecutable = objDialog.FileName;
            }
        }

        private void btnBrowseStorageLocation_Click(object sender, System.Windows.RoutedEventArgs e) {
            var objDialog = new FolderBrowserDialog {
                SelectedPath = _objViewModel.StorageLocation
            };
            var res = objDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK) {
                _objViewModel.StorageLocation = objDialog.SelectedPath;
            }
        }

        private void btnBrowseBackupLocation_Click(object sender, System.Windows.RoutedEventArgs e) {
            var objDialog = new FolderBrowserDialog {
                SelectedPath = _objViewModel.BackupLocation
            };
            var res = objDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK) {
                _objViewModel.BackupLocation = objDialog.SelectedPath;
            }
        }
    }
}