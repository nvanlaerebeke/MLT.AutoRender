﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoRender.Data;
using AutoRender.MLT;
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
    public class WorkspaceOld {
        private static readonly log4net.ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<List<WorkspaceUpdatedEventArgs>> Updated;

        private readonly VideoInfoProvider VideoCache;
        private readonly WorkspaceMonitor Monitor;
        public ConcurrentDictionary<string, WorkspaceItem> WorkspaceItems { get; private set; }

        public WorkspaceOld() {
            Log.Debug("Initializing Workspace");
            if (!Directory.Exists(Settings.NewDirectory)) {
                Log.Debug($"Creating {Settings.NewDirectory}");
                _ = Directory.CreateDirectory(Settings.NewDirectory);
            }
            if (!Directory.Exists(Settings.FinalDirectory)) {
                Log.Debug($"Creating {Settings.FinalDirectory}");
                _ = Directory.CreateDirectory(Settings.FinalDirectory);
            }
            if (!Directory.Exists(Settings.ProjectDirectory)) {
                Log.Debug($"Creating {Settings.ProjectDirectory}");
                _ = Directory.CreateDirectory(Settings.ProjectDirectory);
            }
            if (!Directory.Exists(Settings.TempDirectory)) {
                Log.Debug($"Creating {Settings.TempDirectory}");
                _ = Directory.CreateDirectory(Settings.TempDirectory);
            }

            VideoCache = new VideoInfoProvider();
            WorkspaceItems = new ConcurrentDictionary<string, WorkspaceItem>();
            //Monitor = new WorkspaceMonitor();

            Monitor.Updated += Watcher_Updated;
        }

        public WorkspaceItem Get(Guid pItemID) {
            if (WorkspaceItems.ContainsKey(pItemID.ToString())) {
                if (WorkspaceItems.TryGetValue(pItemID.ToString(), out var objWsItem)) {
                    return objWsItem;
                }
            }
            return null;
        }

        public void ReLoad() {
            var lstCacheBuilding = new List<Task<VideoInfo>>();
            var lstNew = new List<FileInfo>();
            var lstFinal = new List<FileInfo>();
            var lstProjects = new List<FileInfo>();

            var arrNew = Directory.GetFiles(Settings.NewDirectory, "*.mp4").OrderBy(p => p);
            foreach (var strPath in arrNew) {
                lstCacheBuilding.Add(VideoCache.GetAsync(strPath));
                lstNew.Add(new FileInfo(strPath));
            }

            var arrFinal = Directory.GetFiles(Settings.FinalDirectory, "*.mp4").OrderBy(p => p);
            foreach (var strPath in arrFinal) {
                lstCacheBuilding.Add(VideoCache.GetAsync(strPath));
                lstFinal.Add(new FileInfo(strPath));
            }

            var arrProjects = Directory.GetFiles(Settings.ProjectDirectory, "*.mlt").OrderBy(p => p);
            foreach (var strPath in arrProjects) {
                lstProjects.Add(new FileInfo(strPath));
            }

            //wait for cache to be done building
            lstCacheBuilding.ForEach(t => t.Wait());
            //set items
            WorkspaceItems.Clear();
            CreateWorkspaceItems(lstProjects, lstNew, lstFinal);

            //let everyone know this was updated
            var lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            foreach (var objKvp in WorkspaceItems) {
                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objKvp.Value));
            };
            Updated?.Invoke(this, lstUpdates);
        }

        public VideoInfo GetSourceInfo(string pName) {
            return VideoCache.Get(Path.Combine(Settings.NewDirectory, pName));
        }

        private void CreateWorkspaceItems(List<FileInfo> pProjects, List<FileInfo> pNew, List<FileInfo> pFinal) {
            //Do the projects first as they can reference a file that isn't the same name as the project
            foreach (var objP in pProjects) {
                var objProject = new MLTProject(objP.FullName, VideoCache);
                var objNewFile = pNew.FirstOrDefault(f => f.Name == Path.GetFileName(objProject.SourcePath));
                var objFinalFile = pFinal.FirstOrDefault(f => f.Name == objProject.TargetName);

                AddItem(
                    objProject,
                    (objNewFile != null) ? VideoCache.Get(objNewFile.FullName) : null,
                    (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null
                );

                if (objNewFile != null) { pNew.Remove(objNewFile); }
                if (objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (var objNew in pNew) {
                var objFinalFile = pFinal.FirstOrDefault(f => f.Name == objNew.Name);
                AddItem(null, VideoCache.Get(objNew.FullName), (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null);
                if (objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (var objFinal in pFinal) {
                AddItem(null, null, VideoCache.Get(objFinal.FullName));
            }
        }

        private void Watcher_Updated(WorkspaceType pType, List<FSEventInfo> e) {
            switch (pType) {
                case WorkspaceType.Project:
                    ProjectsUpdated(e);
                    break;

                case WorkspaceType.New:
                    NewUpdated(e);
                    break;

                case WorkspaceType.Final:
                    FinalUpdated(e);
                    break;
            }
        }

        private void FinalUpdated(List<FSEventInfo> pUpdates) {
            var lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            pUpdates.ForEach(u => {
                switch (u.Args.ChangeType) {
                    case WatcherChangeTypes.Changed:
                        WorkspaceItems.Where(i => (
                            i.Value.Project != null &&
                            i.Value.Project.TargetPath == u.Args.FullPath
                        ) || (
                           i.Value.Final != null &&
                        i.Value.Final.Path == u.Args.FullPath
                        )).ToList().ForEach(i => {
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                        });
                        break;

                    case WatcherChangeTypes.Created:
                        //Do not update when there is a running task for that file, it's no use doing that
                        var lstWsItems = WorkspaceItems.Where(i =>
                            i.Value.Project != null &&
                            i.Value.Project.TargetPath == u.Args.FullPath
                        ).ToList();

                        lstWsItems.ForEach(i => {
                            if (i.Value.Project != null && i.Value.Project.Status != ProjectStatus.Busy) {
                                i.Value.Project.Reload();
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                            }
                        });
                        if (lstWsItems.Count == 0) {
                            var objWSItem = AddItem(null, null, VideoCache.Get(u.Args.FullPath));
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objWSItem));
                        }
                        break;

                    case WatcherChangeTypes.Deleted:
                        WorkspaceItems.Where(i =>
                            i.Value.Project != null &&
                            i.Value.Project.TargetPath == u.Args.FullPath
                        ).ToList().ForEach(i => {
                            i.Value.Project.Reload();
                            i.Value.UpdateFinal(null);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i.Value));
                        });
                        WorkspaceItems.Where(i =>
                            i.Value.Project == null &&
                            i.Value.Final != null &&
                            i.Value.Final.Path == u.Args.FullPath
                        ).ToList().ForEach(i => {
                            RemoveItem(i.Value);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i.Value));
                        });
                        break;

                    case WatcherChangeTypes.Renamed:
                        //not supported
                        break;
                }
            });
            Updated?.Invoke(this, lstUpdates);
        }

        private void NewUpdated(List<FSEventInfo> pUpdates) {
            var lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            pUpdates.ForEach(u => {
                switch (u.Args.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        var objWsItems = WorkspaceItems.Where(i =>
                            (
                                i.Value.Project != null &&
                                i.Value.Project.TargetPath.Equals(u.Args.FullPath)
                            ) || (
                                i.Value.New != null &&
                                i.Value.New.Path.Equals(u.Args.FullPath)
                            )
                        ).ToList();
                        if (objWsItems.Count > 0) {
                            objWsItems.ForEach(i => {
                                i.Value.UpdateNew(VideoCache.Get(u.Args.FullPath));
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                            });
                        } else {
                            var objItem = AddItem(null, VideoCache.Get(u.Args.FullPath), null);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objItem));
                        }
                        break;

                    case WatcherChangeTypes.Deleted:
                        WorkspaceItems.Where(i =>
                             (
                                i.Value.Project != null &&
                                i.Value.Project.TargetPath.Equals(u.Args.FullPath)
                            ) || (
                                i.Value.New != null &&
                                i.Value.New.Path.Equals(u.Args.FullPath)
                            )
                        ).ToList().ForEach(i => {
                            if (i.Value.Project == null && i.Value.Final == null) {
                                RemoveItem(i.Value);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i.Value));
                            } else {
                                i.Value.UpdateNew(null);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                            }
                        });
                        break;

                    case WatcherChangeTypes.Renamed:
                        //not supported
                        break;
                }
            });
            Updated?.Invoke(this, lstUpdates);
        }

        private void ProjectsUpdated(List<FSEventInfo> pUpdates) {
            var lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            pUpdates.ForEach(u => {
                var objWSItems = WorkspaceItems.Where(i => i.Value.Project != null && i.Value.Project.FullPath.Equals(u.Args.FullPath)).ToList();
                switch (u.Args.ChangeType) {
                    case WatcherChangeTypes.Deleted:
                        objWSItems.ForEach(i => {
                            var objAction = WorkspaceAction.Deleted;
                            if (i.Value.New == null && i.Value.Final == null) {
                                RemoveItem(i.Value);
                            } else {
                                objAction = WorkspaceAction.Updated;
                                i.Value.UpdateProject(null);
                            }
                            lstUpdates.Add(GetUpdateEvent(objAction, i.Value));
                        });
                        break;

                    case WatcherChangeTypes.Changed:
                        objWSItems.ForEach(i => {
                            i.Value.UpdateProject(new MLTProject(u.Args.FullPath, VideoCache));
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                        });
                        break;

                    case WatcherChangeTypes.Created:
                        var objProject = new MLTProject(u.Args.FullPath, VideoCache);
                        var lstWsItems = WorkspaceItems.Where(i => i.Value.New != null && i.Value.New.Path == objProject.SourcePath).ToList();
                        if (lstWsItems.Count > 0) {
                            lstWsItems.ForEach(i => {
                                i.Value.UpdateProject(objProject);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                            });
                        } else {
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, new WorkspaceItem(objProject, null, null)));
                        }
                        break;

                    case WatcherChangeTypes.Renamed:
                        //not supported
                        break;
                }
            });
            Updated?.Invoke(this, lstUpdates);
        }

        private WorkspaceUpdatedEventArgs GetUpdateEvent(WorkspaceAction pType, WorkspaceItem pWorkspaceItem) {
            return new WorkspaceUpdatedEventArgs(
                new Data.WorkspaceItem(pWorkspaceItem.ID) {
                    Final = pWorkspaceItem.Final,
                    New = pWorkspaceItem.New,
                    Project = pWorkspaceItem.Project?.GetProject()
                },
                pType
            );
        }

        private WorkspaceItem AddItem(MLTProject pProject, VideoInfo pNew, VideoInfo pFinal) {
            var obj = new WorkspaceItem(pProject, pNew, pFinal);
            obj.Updated += WorkspaceItemUpdated;
            if (!WorkspaceItems.TryAdd(obj.ID.ToString(), obj)) {
                //ToDo: error handling
            }
            return obj;
        }

        private void RemoveItem(WorkspaceItem pItem) {
            pItem.Updated -= WorkspaceItemUpdated;
            if (!WorkspaceItems.TryRemove(pItem.ID.ToString(), out _)) {
                //ToDo: Error Handling
            }
        }

        private void WorkspaceItemUpdated(object sender, WorkspaceItem e) {
            Log.Info("Workspace was updated, sending updated workspaceitem");
            Updated?.Invoke(this, new List<WorkspaceUpdatedEventArgs>()
                { new WorkspaceUpdatedEventArgs(e.GetWorkspaceItem(), WorkspaceAction.Updated) }
            );
        }
    }
}