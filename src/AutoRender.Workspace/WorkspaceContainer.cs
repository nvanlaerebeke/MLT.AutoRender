using AutoRender.Data;
using AutoRender.Workspace.Monitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoRender.Workspace {

    public delegate void WorkspaceItemUpdated(object sender, List<WorkspaceUpdatedEventArgs> e);

    public class WorkspaceContainer {

        public event WorkspaceItemUpdated Updated;

        private static WorkspaceMonitor _objWorkspaceMonitor;

        public List<WorkspaceItem> WorkspaceItems {
            get { return _objWorkspaceMonitor.WorkspaceItems; }
        }

        internal WorkspaceContainer() {
            Cleanup();

            if (!Directory.Exists(Settings.NewDirectory)) { Directory.CreateDirectory(Settings.NewDirectory); }
            if (!Directory.Exists(Settings.LogDirectory)) { Directory.CreateDirectory(Settings.LogDirectory); }
            if (!Directory.Exists(Settings.FinalDirectory)) { Directory.CreateDirectory(Settings.FinalDirectory); }
            if (!Directory.Exists(Settings.ProjectDirectory)) { Directory.CreateDirectory(Settings.ProjectDirectory); }
            if (!Directory.Exists(Settings.TempDirectory)) { Directory.CreateDirectory(Settings.TempDirectory); }

            //Load and Monitor for changes
            _objWorkspaceMonitor = new WorkspaceMonitor();
            _objWorkspaceMonitor.Updated += (object sender, List<WorkspaceUpdatedEventArgs> e) => { Updated?.Invoke(sender, e); };
        }

        public WorkspaceItem Get(Guid pProjectID) {
            lock (_objWorkspaceMonitor.WorkspaceItems) {
                return _objWorkspaceMonitor.WorkspaceItems.FirstOrDefault(i => i.ID.Equals(pProjectID));
            }
        }

        public void Reload() {
            _objWorkspaceMonitor.Reload();
        }

        private void Cleanup() {
            if (Directory.Exists(Settings.TempDirectory)) {
                var arrFiles = Directory.GetFiles(Settings.TempDirectory);
                try {
                    foreach (var strFile in arrFiles) {
                        File.Delete(strFile);
                    }
                } catch { }
            }
        }
    }
}