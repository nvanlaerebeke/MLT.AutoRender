using System.IO;

namespace AutoRender.Workspace.Monitor {

    public class NewMonitor {
        private readonly Monitor _objMonitor;

        internal event FSEvent Changed;

        public NewMonitor(string pPath) {
            _objMonitor = new Monitor(new FileSystemWatcher(pPath, "*.mp4"));
            _objMonitor.Changed += _objMonitor_Changed;
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