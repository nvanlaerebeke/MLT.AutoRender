using AutoRender.Lib.Monitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoRender.Lib {
    public delegate void WorkspaceItemUpdated(object sender, List<WorkspaceUpdatedEventArgs> e);
    public class Workspace {
        public static event WorkspaceItemUpdated Updated;
        private static WorkspaceMonitor _objWorkspaceMonitor;

        public static List<WorkspaceItem> WorkspaceItems {
            get { return _objWorkspaceMonitor.WorkspaceItems; }
        }

        public static void StartMonitoring() {
            if (!Directory.Exists(Settings.NewDirectory)) { Directory.CreateDirectory(Settings.NewDirectory); }
            if (!Directory.Exists(Settings.LogDirectory)) { Directory.CreateDirectory(Settings.LogDirectory); }
            if (!Directory.Exists(Settings.FinalDirectory)) { Directory.CreateDirectory(Settings.FinalDirectory); }
            if (!Directory.Exists(Settings.ProjectDirectory)) { Directory.CreateDirectory(Settings.ProjectDirectory); }
            if (!Directory.Exists(Settings.TempDirectory)) { Directory.CreateDirectory(Settings.TempDirectory); }

            //Load and Monitor for changes
            _objWorkspaceMonitor = new WorkspaceMonitor();
            _objWorkspaceMonitor.Updated += (object sender, List<WorkspaceUpdatedEventArgs> e) => { Updated?.Invoke(sender, e); };
        }

        public static WorkspaceItem Get(Guid pProjectID) {
            lock (_objWorkspaceMonitor.WorkspaceItems) {
                return _objWorkspaceMonitor.WorkspaceItems.FirstOrDefault(i => i.ID.Equals(pProjectID));
            }
        }

        public static void Reload() {
            _objWorkspaceMonitor.Reload();
        }
    }
}