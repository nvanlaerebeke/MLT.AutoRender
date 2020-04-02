using System.IO;

namespace AutoRender.Workspace.Monitor {

    public class ProjectMonitor {
        private readonly Monitor _objMonitor;

        internal event FSEvent Changed;

        public ProjectMonitor(string pPath) {
            _objMonitor = new Monitor(new FileSystemWatcher(pPath, "*.mlt"));
            _objMonitor.Changed += _objMonitor_Changed;
            _objMonitor.Start();
        }

        private void _objMonitor_Changed(System.Collections.Generic.List<FSEventInfo> pEvents) {
            Changed?.Invoke(pEvents);
        }

        public void Start() {
            _objMonitor.Start();
        }

        public void Stop() {
            _objMonitor.Stop();
        }
    }
}