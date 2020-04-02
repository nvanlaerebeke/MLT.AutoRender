using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoRender.Data;
using AutoRender.Video;
using AutoRender.Workspace.Monitor;
using log4net;

namespace AutoRender.Workspace {

    /// <summary>
    /// ToDo: do not add the workspace watcher here
    /// the workspace is not depended on the watcher, it's the other way around
    ///
    /// ToDo: constuctor should not do anything, create a workspace initialize method
    /// </summary>
    public class Workspace {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<List<WorkspaceUpdatedEventArgs>> Updated;

        public event EventHandler Reloaded;

        private readonly VideoInfoProvider VideoInfoProvider;
        private readonly WorkspaceUpdateCollector UpdateCollector;
        private readonly WorkspaceContainer WorkspaceContainer;

        public Workspace(string pNewDir, string pFinalDir, string pProjectDir, string pTempDir, VideoInfoProvider pVideoInfoProvider) {
            VideoInfoProvider = pVideoInfoProvider;
            WorkspaceContainer = new WorkspaceContainer();

            Init(pNewDir, pFinalDir, pProjectDir, pTempDir);

            UpdateCollector = new WorkspaceUpdateCollector(
                WorkspaceContainer,
                new WorkspaceMonitor(Settings.NewDirectory, Settings.FinalDirectory, Settings.ProjectDirectory),
                VideoInfoProvider
            );
            UpdateCollector.Updated += UpdateCollector_Updated;
            UpdateCollector.ReloadRequired += UpdateCollector_ReloadRequired;

            Reload();
        }

        private void Init(string pNewDir, string pFinalDir, string pProjectDir, string pTempDir) {
            Log.Debug("Initializing Workspace");
            if (!Directory.Exists(pNewDir)) {
                Log.Debug($"Creating {pNewDir}");
                _ = Directory.CreateDirectory(pNewDir);
            }
            if (!Directory.Exists(pFinalDir)) {
                Log.Debug($"Creating {pFinalDir}");
                _ = Directory.CreateDirectory(pFinalDir);
            }
            if (!Directory.Exists(pProjectDir)) {
                Log.Debug($"Creating {pProjectDir}");
                _ = Directory.CreateDirectory(pProjectDir);
            }
            if (!Directory.Exists(pTempDir)) {
                Log.Debug($"Creating {pTempDir}");
                _ = Directory.CreateDirectory(pTempDir);
            }
        }

        public WorkspaceItem Get(Guid pItemID) {
            return WorkspaceContainer.Get(pItemID);
        }

        public List<WorkspaceItem> GetAll() {
            return WorkspaceContainer.GetAll();
        }

        public void Reload() {
            UpdateCollector.Stop();
            var items = new WorkspaceScanner(Settings.NewDirectory, Settings.FinalDirectory, Settings.ProjectDirectory, VideoInfoProvider).Scan();

            WorkspaceContainer.Clear();
            items.ForEach(i => {
                WorkspaceContainer.Add(i);
            });
            Reloaded?.Invoke(this, new EventArgs());
            UpdateCollector.Start();
        }

        private void UpdateCollector_Updated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            Updated?.Invoke(this, e);
        }

        private void UpdateCollector_ReloadRequired(object sender, EventArgs e) {
            Reload();
        }
    }
}