using AutoRender.Data;
using System.IO;

namespace AutoRender.Workspace.Monitor {

    public class ProjectMonitor {
        private Monitor _objMonitor;

        internal event FSEvent Changed;

        public ProjectMonitor() {
            _objMonitor = new Monitor(new FileSystemWatcher(Settings.ProjectDirectory, "*.mlt"));
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