using System;
using System.Windows;
using AutoRender.Client.Config;

namespace AutoRender {
    /// <summary>
    /// Interaction logic for Backup.xaml
    /// </summary>
    public partial class Backup : BaseWindow {
        private readonly AutoRender.Client.Backup.Backup Rsync;
        public Backup() {
            InitializeComponent();

            Closed += Backup_Closed;

            Rsync = new AutoRender.Client.Backup.Backup(Settings.StorageLocation, Settings.BackupLocation);
            Rsync.Progress += Rsync_Progress;
            Rsync.StatusChanged += Rsync_StatusChanged;
            Rsync.Start();
        }

        private void Rsync_StatusChanged(object sender, CrazyUtils.ProcessStatus e) {
            Application.Current.Dispatcher.Invoke(() => {
                if (e == CrazyUtils.ProcessStatus.Done) {
                    txtActivity.AppendText("Backup done successfully" + Environment.NewLine);
                } else if (e == CrazyUtils.ProcessStatus.Failed) {
                    txtActivity.AppendText("Backup failed" + Environment.NewLine);
                } else {
                    txtActivity.AppendText("Process status changed to " + e.ToString() + Environment.NewLine);
                }
                txtActivity.ScrollToEnd();
            });
        }

        private void Rsync_Progress(object sender, string e) {
            Application.Current.Dispatcher.Invoke(() => {
                txtActivity.AppendText(e);
                txtActivity.ScrollToEnd();
            });
        }

        private void Backup_Closed(object sender, EventArgs e) {
            Rsync.Dispose();
        }
    }
}
