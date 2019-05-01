using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoRender.Data;
using AutoRender.MLT;
using AutoRender.Video;
using AutoRender.Workspace.Monitor;

namespace AutoRender.Workspace {
    public class Workspace {
        public event EventHandler<List<WorkspaceUpdatedEventArgs>> Updated;

        private readonly VideoInfoCache VideoCache;
        private readonly WorkspaceWatcher Watcher;
        public ConcurrentDictionary<string, WorkspaceItem> WorkspaceItems { get; private set; }

        public Workspace() {
            VideoCache = new VideoInfoCache();
            WorkspaceItems = new ConcurrentDictionary<string, WorkspaceItem>();
            Watcher = new WorkspaceWatcher();

            Watcher.Updated += Watcher_Updated;
        }

        public WorkspaceItem Get(Guid pItemID) {
            if(WorkspaceItems.ContainsKey(pItemID.ToString())) {
                if(WorkspaceItems.TryGetValue(pItemID.ToString(), out WorkspaceItem objWsItem)) {
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
            foreach (string strPath in arrNew) {
                lstCacheBuilding.Add(VideoCache.GetAsync(strPath));
                lstNew.Add(new FileInfo(strPath));
            }

            var arrFinal = Directory.GetFiles(Settings.FinalDirectory, "*.mp4").OrderBy(p => p);
            foreach (string strPath in arrFinal) {
                lstCacheBuilding.Add(VideoCache.GetAsync(strPath));
                lstFinal.Add(new FileInfo(strPath));
            }

            var arrProjects = Directory.GetFiles(Settings.ProjectDirectory, "*.mlt").OrderBy(p => p);
            foreach (string strPath in arrProjects) {
                lstProjects.Add(new FileInfo(strPath));
            }

            //wait for cache to be done building
            lstCacheBuilding.ForEach(t => t.Wait());
            //set items
            WorkspaceItems.Clear();
            CreateWorkspaceItems(lstProjects, lstNew, lstFinal);

            //let everyone know this was updated
            List<WorkspaceUpdatedEventArgs> lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            foreach(var objKvp in WorkspaceItems) { 
                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objKvp.Value));
            };
            Updated?.Invoke(this, lstUpdates);
        }

        private void CreateWorkspaceItems(List<FileInfo> pProjects, List<FileInfo> pNew, List<FileInfo> pFinal) {
            //Do the projects first as they can reference a file that isn't the same name as the project
            foreach (FileInfo objP in pProjects) {
                MLTProject objProject = new MLTProject(objP.FullName, VideoCache);
                FileInfo objNewFile = pNew.FirstOrDefault(f => f.Name == Path.GetFileName(objProject.SourcePath));
                FileInfo objFinalFile = pFinal.FirstOrDefault(f => f.Name == objProject.TargetName);

                AddItem(
                    objProject,
                    (objNewFile != null) ? VideoCache.Get(objNewFile.FullName) : null,
                    (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null
                );


                if(objNewFile != null) { pNew.Remove(objNewFile); }
                if(objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (FileInfo objNew in pNew) {
                var objFinalFile = pFinal.FirstOrDefault(f => f.Name == objNew.Name);
                AddItem(null, VideoCache.Get(objNew.FullName), (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null);
                if(objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (FileInfo objFinal in pFinal) {
                AddItem(null, null, VideoCache.Get(objFinal.FullName));
            }
        }

        void Watcher_Updated(WorkspaceType pType, List<FSEventInfo> e) {
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
            List<WorkspaceUpdatedEventArgs> lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            pUpdates.ForEach(u => {
                switch (u.Args.ChangeType) {
                    case WatcherChangeTypes.Changed:
                        WorkspaceItems.Where(i =>  (
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
                            if(i.Value.Project != null && i.Value.Project.Status != ProjectStatus.Busy) {
                                i.Value.Project.Reload();
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i.Value));
                            }
                        });
                        if(lstWsItems.Count == 0) {
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
            List<WorkspaceUpdatedEventArgs> lstUpdates = new List<WorkspaceUpdatedEventArgs>();
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
                        if(objWsItems.Count > 0) {
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
                            if(i.Value.Project == null && i.Value.Final == null) {
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
            List<WorkspaceUpdatedEventArgs> lstUpdates = new List<WorkspaceUpdatedEventArgs>();
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
                new Data.WorkspaceItem() {
                    Final = pWorkspaceItem.Final,
                    ID = pWorkspaceItem.ID,
                    New = pWorkspaceItem.New,
                    Project = pWorkspaceItem.Project?.GetProject()
                },
                pType
            );
        }

        private WorkspaceItem AddItem(MLTProject pProject, VideoInfo pNew, VideoInfo pFinal) {
            var obj = new WorkspaceItem(pProject, pNew, pFinal);
            obj.Updated += WorkspaceItemUpdated;
            if(!WorkspaceItems.TryAdd(obj.ID.ToString(), obj)) {
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

        void WorkspaceItemUpdated(object sender, WorkspaceItem e) {
            /*switch (e.Project.Status) {
                case ProjectStatus.Error:
                case ProjectStatus.Finished:
                case ProjectStatus.SourceInvalid:
                case ProjectStatus.SourceMissing:
                case ProjectStatus.TargetExists:
                case ProjectStatus.TargetInvalid:
                    if (e.Project.TargetExists) {
                        var objWsItem = WorkspaceItems.Where(i => i.Value.Project != null && i.Value.Project.ID.ToString().Equals(e.Project.ID.ToString())).FirstOrDefault();

                    }
                    break;
            }*/
            Updated?.Invoke(this, new List<WorkspaceUpdatedEventArgs>()
                { new WorkspaceUpdatedEventArgs(e.GetWorkspaceItem(), WorkspaceAction.Updated) }
            );
        }

    }
}
