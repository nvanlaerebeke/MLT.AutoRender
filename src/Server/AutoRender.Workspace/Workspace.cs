using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoRender.Data;
using AutoRender.Server.Config;
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
        private WorkspaceUpdateCollector UpdateCollector;
        private WorkspaceContainer WorkspaceContainer;
        private WorkspaceScanner WorkspaceScanner;

        private string NewDirectory { get; set; }
        private string FinalDirectory { get; set; }
        private string ProjectDirectory { get; set; }
        private string TempDirectory { get; set; }

        public Workspace(string pNewDirectory, string pFinalDirectory, string pProjectDirectory, string pTempDir, VideoInfoProvider pVideoInfoProvider) {
            VideoInfoProvider = pVideoInfoProvider;

            NewDirectory = pNewDirectory;
            FinalDirectory = pFinalDirectory;
            ProjectDirectory = pProjectDirectory;
            TempDirectory = pTempDir;

            Init();
        }

        private void Init() {
            WorkspaceContainer = new WorkspaceContainer();
            WorkspaceScanner = new WorkspaceScanner(NewDirectory, ProjectDirectory, FinalDirectory, VideoInfoProvider);

            Log.Debug("Initializing Workspace");
            if (!Directory.Exists(NewDirectory)) {
                Log.Debug($"Creating {NewDirectory}");
                _ = Directory.CreateDirectory(NewDirectory);
            }
            if (!Directory.Exists(FinalDirectory)) {
                Log.Debug($"Creating {FinalDirectory}");
                _ = Directory.CreateDirectory(FinalDirectory);
            }
            if (!Directory.Exists(ProjectDirectory)) {
                Log.Debug($"Creating {ProjectDirectory}");
                _ = Directory.CreateDirectory(ProjectDirectory);
            }
            if (!Directory.Exists(TempDirectory)) {
                Log.Debug($"Creating {TempDirectory}");
                _ = Directory.CreateDirectory(TempDirectory);
            }

            UpdateCollector = new WorkspaceUpdateCollector(
                WorkspaceContainer,
                new WorkspaceMonitor(NewDirectory, FinalDirectory, ProjectDirectory),
                VideoInfoProvider
            );
            UpdateCollector.Updated += UpdateCollector_Updated;
            UpdateCollector.ReloadRequired += UpdateCollector_ReloadRequired;
            Settings.WorkspaceSourceUpdated += Settings_WorkspaceSourceUpdated;

            Reload();
        }

        public WorkspaceItem Get(Guid pItemID) {
            return WorkspaceContainer.Get(pItemID);
        }

        public List<WorkspaceItem> GetAll() {
            return WorkspaceContainer.GetAll();
        }

        public void Reload() {
            UpdateCollector.Stop();
            var items = WorkspaceScanner.Scan();

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

        private void Settings_WorkspaceSourceUpdated(object sender, EventArgs e) {
            VideoInfoProvider.Clear();
            UpdateCollector.Updated -= UpdateCollector_Updated;
            UpdateCollector.ReloadRequired -= UpdateCollector_ReloadRequired;
            Settings.WorkspaceSourceUpdated -= Settings_WorkspaceSourceUpdated;

            FinalDirectory = Settings.FinalDirectory;
            NewDirectory = Settings.NewDirectory;
            ProjectDirectory = Settings.ProjectDirectory;

            Init();
        }
    }
}