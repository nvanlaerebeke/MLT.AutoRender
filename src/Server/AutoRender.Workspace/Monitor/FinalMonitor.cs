using AutoRender.Data;
using System;
using System.IO;

namespace AutoRender.Workspace.Monitor {

    public class FinalMonitor {
        private Monitor _objMonitor;

        internal event FSEvent Changed;

        public FinalMonitor() {
            _objMonitor = new Monitor(new FileSystemWatcher(Settings.FinalDirectory, "*.mp4"));
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