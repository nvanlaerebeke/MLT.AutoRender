using System;
using System.Threading;
using CrazyUtils;

namespace AutoRender.Client.Backup {
    public class Backup : IDisposable {
        private readonly Rsync Rsync;

        public event EventHandler<string> Progress;
        public event EventHandler<ProcessStatus> StatusChanged;

        public Backup(string pFrom, string pTo) {
            Rsync = new Rsync(pFrom, pTo);
            Rsync.Progress += Rsync_Progress;
            Rsync.StatusChanged += Rsync_StatusChanged;
        }

        public void Start() {
            new Thread(() => {
                Rsync.Start();
            }) { IsBackground = true }.Start();
        }

        private void Rsync_StatusChanged(object sender, ProcessStatus e) {
            StatusChanged?.Invoke(this, e);
        }

        private void Rsync_Progress(object sender, string e) {
            Progress?.Invoke(this, e);
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            Rsync.Progress -= Rsync_Progress;
            Rsync.StatusChanged -= Rsync_StatusChanged;

            Rsync.Dispose();
        }
    }
}
