using System;
using System.Collections.Generic;

namespace AutoRender.Workspace.Monitor {
    internal delegate void WorkspaceUpdated(WorkspaceType pType, List<FSEventInfo> pChanges);

    internal enum WorkspaceType { 
        Project,
        New,
        Final
    }

    internal class WorkspaceWatcher {
        public event WorkspaceUpdated Updated;

        #region private Fields

        private readonly ProjectMonitor _objProjectMonitor;
        private readonly FinalMonitor _objFinalMonitor;
        private readonly NewMonitor _objNewMonitor;

        #endregion private Fields

        public WorkspaceWatcher() {
            _objProjectMonitor = new ProjectMonitor();
            _objFinalMonitor = new FinalMonitor();
            _objNewMonitor = new NewMonitor();

            _objProjectMonitor.Changed += _objProjectMonitor_Changed;
            _objFinalMonitor.Changed += _objFinalMonitor_Changed;
            _objNewMonitor.Changed += _objNewMonitor_Changed;

            _objProjectMonitor.Start();
            _objNewMonitor.Start();
            _objFinalMonitor.Start();
        }

        void _objNewMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.New, pEvents);
        }


        void _objFinalMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.Final, pEvents);
        }


        void _objProjectMonitor_Changed(List<FSEventInfo> pEvents) {
            Updated?.Invoke(WorkspaceType.Project, pEvents);
        }
    }
}
