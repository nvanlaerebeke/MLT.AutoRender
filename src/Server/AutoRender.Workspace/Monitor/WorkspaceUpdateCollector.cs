using System;
using System.Collections.Generic;
using System.Timers;
using AutoRender.Data;
using AutoRender.Video;

namespace AutoRender.Workspace.Monitor {

    internal class WorkspaceUpdateCollector {

        public event EventHandler<List<WorkspaceUpdatedEventArgs>> Updated;

        public event EventHandler ReloadRequired;

        private readonly List<WorkspaceUpdatedEventArgs> Updates;
        private readonly WorkspaceMonitor Monitor;
        private readonly WorkspaceContainer Container;
        private readonly VideoInfoProvider VideoInfoProvider;

        private readonly Timer _objWaitTimer;

        public WorkspaceUpdateCollector(WorkspaceContainer pContainer, WorkspaceMonitor pMonitor, VideoInfoProvider pVideoInfoProvider) {
            Container = pContainer;
            Monitor = pMonitor;
            VideoInfoProvider = pVideoInfoProvider;

            Updates = new List<WorkspaceUpdatedEventArgs>();

            _objWaitTimer = new Timer(2000);
            _objWaitTimer.Elapsed += (sender, e) => {
                _objWaitTimer.Stop();
                lock (Updates) {
                    if (Updates.Count > 0) {
                        var lstCopy = new List<WorkspaceUpdatedEventArgs>(Updates);
                        Updates.Clear();
                        Updated?.Invoke(this, lstCopy);
                    }
                }
            };
            _objWaitTimer.AutoReset = false;
        }

        public void Start() {
            Container.WorkspaceItemUpdated += WorkspaceContainer_WorkspaceItemUpdated;
            Monitor.Updated += Monitor_Updated;
            Monitor.Start();
        }

        public void Stop() {
            Container.WorkspaceItemUpdated -= WorkspaceContainer_WorkspaceItemUpdated;
            Monitor.Updated -= Monitor_Updated;
            Monitor.Stop();
            Updates.Clear();
        }

        private void WorkspaceContainer_WorkspaceItemUpdated(object sender, WorkspaceUpdatedEventArgs e) {
            Add(e);
        }

        /// <summary>
        /// Apply changes on the WorkspaceContainer, the update event handler will get trigger if anything was changed
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pEvents"></param>
        private void Monitor_Updated(WorkspaceType pType, List<FSEventInfo> pEvents) {
            try {
                Updater.Apply(Container, VideoInfoProvider, pType, pEvents);
            } catch (Exception) {
                ReloadRequired?.Invoke(this, new EventArgs());
            }
        }

        private void Add(WorkspaceUpdatedEventArgs pEvent) {
            _objWaitTimer.Start();
            Updates.Add(pEvent);
        }
    }
}