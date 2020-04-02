using System.Collections.Generic;

namespace AutoRender.Workspace.Monitor {

    internal delegate void WorkspaceUpdated(WorkspaceType pType, List<FSEventInfo> pEvents);

    internal class WorkspaceMonitor {

        public event WorkspaceUpdated Updated;

        private readonly ProjectMonitor _objProjectMonitor;
        private readonly FinalMonitor _objFinalMonitor;
        private readonly NewMonitor _objNewMonitor;

        public WorkspaceMonitor(string pNewDir, string pFinalDir, string pProjectDir) {
            _objProjectMonitor = new ProjectMonitor(pProjectDir);
            _objFinalMonitor = new FinalMonitor(pFinalDir);
            _objNewMonitor = new NewMonitor(pNewDir);
        }

        public void Start() {
            _objProjectMonitor.Changed += _objProjectMonitor_Changed;
            _objFinalMonitor.Changed += _objFinalMonitor_Changed;
            _objNewMonitor.Changed += _objNewMonitor_Changed;

            _objProjectMonitor.Start();
            _objNewMonitor.Start();
            _objFinalMonitor.Start();
        }

        public void Stop() {
            _objProjectMonitor.Changed -= _objProjectMonitor_Changed;
            _objFinalMonitor.Changed -= _objFinalMonitor_Changed;
            _objNewMonitor.Changed -= _objNewMonitor_Changed;

            _objProjectMonitor.Stop();
            _objNewMonitor.Stop();
            _objFinalMonitor.Stop();
        }

        private void _objNewMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.New, pEvents);
        }

        private void _objFinalMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.Final, pEvents);
        }

        private void _objProjectMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.Project, pEvents);
        }
    }
}