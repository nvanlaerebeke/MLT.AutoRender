using System;
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
        public List<WorkspaceItem> WorkspaceItems { get; private set; }

        public Workspace() {
            VideoCache = new VideoInfoCache();
            WorkspaceItems = new List<WorkspaceItem>();
            Watcher = new WorkspaceWatcher();

            Watcher.Updated += Watcher_Updated;
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
            WorkspaceItems = CreateWorkspaceItems(lstProjects, lstNew, lstFinal);

            //let everyone know this was updated
            List<WorkspaceUpdatedEventArgs> lstUpdates = new List<WorkspaceUpdatedEventArgs>();
            WorkspaceItems.ForEach(i => {
                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, i));
            });
            Updated?.Invoke(this, lstUpdates);
        }

        private List<WorkspaceItem> CreateWorkspaceItems(List<FileInfo> pProjects, List<FileInfo> pNew, List<FileInfo> pFinal) {
            List<WorkspaceItem> lstWsItems = new List<WorkspaceItem>();

            //Do the projects first as they can reference a file that isn't the same name as the project
            foreach (FileInfo objP in pProjects) {
                MLTProject objProject = new MLTProject(objP.FullName, VideoCache);
                FileInfo objNewFile = pNew.FirstOrDefault(f => f.Name == Path.GetFileName(objProject.SourcePath));
                FileInfo objFinalFile = pFinal.FirstOrDefault(f => f.Name == objProject.TargetName);

                lstWsItems.Add(
                    new WorkspaceItem(
                        objProject,
                        (objNewFile != null) ? VideoCache.Get(objNewFile.FullName) : null,
                        (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null
                    )
                );

                if(objNewFile != null) { pNew.Remove(objNewFile); }
                if(objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (FileInfo objNew in pNew) {
                var objFinalFile = pFinal.FirstOrDefault(f => f.Name == objNew.Name);
                lstWsItems.Add(
                    new WorkspaceItem(
                        null,
                        VideoCache.Get(objNew.FullName),
                        (objFinalFile != null) ? VideoCache.Get(objFinalFile.FullName) : null
                    )
                );
                if(objFinalFile != null) { pFinal.Remove(objFinalFile); }
            }

            foreach (FileInfo objFinal in pFinal) {
                lstWsItems.Add(new WorkspaceItem(null, null, VideoCache.Get(objFinal.FullName)));
            }
            return lstWsItems;
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
                            i.Project != null &&
                            i.Project.TargetPath == u.Args.FullPath
                        ) || (
                           i.Final != null && 
                        i.Final.Path == u.Args.FullPath
                        )).ToList().ForEach(i => {
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
                        });
                        break;
                    case WatcherChangeTypes.Created:
                        //Do not update when there is a running task for that file, it's no use doing that
                        var lstWsItems = WorkspaceItems.Where(i =>
                            i.Project != null &&
                            i.Project.TargetPath == u.Args.FullPath
                        ).ToList();
                            
                        lstWsItems.ForEach(i => { 
                            if(i.Project != null && i.Project.Status != ProjectStatus.Busy) {
                                i.Project.Reload();
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
                            }
                        });
                        if(lstWsItems.Count == 0) {
                            var objWSItem = new WorkspaceItem(null, null, VideoCache.Get(u.Args.FullPath));
                            WorkspaceItems.Add(objWSItem);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objWSItem));
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                        WorkspaceItems.Where(i =>
                            i.Project != null &&
                            i.Project.TargetPath == u.Args.FullPath
                        ).ToList().ForEach(i => {
                            i.Project.Reload();
                            i.UpdateFinal(null);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i));
                        });
                        WorkspaceItems.Where(i => 
                            i.Project == null && 
                            i.Final != null && 
                            i.Final.Path == u.Args.FullPath
                        ).ToList().ForEach(i => {
                            WorkspaceItems.Remove(i);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i));
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
                                i.Project != null &&
                                i.Project.TargetPath.Equals(u.Args.FullPath)
                            ) || (
                                i.New != null &&
                                i.New.Path.Equals(u.Args.FullPath)
                            )
                        ).ToList();
                        if(objWsItems.Count > 0) {
                            objWsItems.ForEach(i => {
                                i.UpdateNew(VideoCache.Get(u.Args.FullPath));
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
                            });
                        } else {
                            var objItem = new WorkspaceItem(null, VideoCache.Get(u.Args.FullPath), null);
                            WorkspaceItems.Add(objItem);
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.New, objItem));
                        }
                        break;
                    case WatcherChangeTypes.Deleted:
                        WorkspaceItems.Where(i =>
                             (
                                i.Project != null && 
                                i.Project.TargetPath.Equals(u.Args.FullPath)
                            ) || (
                                i.New != null && 
                                i.New.Path.Equals(u.Args.FullPath)
                            )
                        ).ToList().ForEach(i => { 
                            if(i.Project == null && i.Final == null) {
                                WorkspaceItems.Remove(i);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Deleted, i));

                            } else {
                                i.UpdateNew(null);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
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
                var objWSItems = WorkspaceItems.Where(i => i.Project != null && i.Project.FullPath.Equals(u.Args.FullPath)).ToList();
                switch (u.Args.ChangeType) {
                    case WatcherChangeTypes.Deleted:
                        objWSItems.ForEach(i => {
                            var objAction = WorkspaceAction.Deleted;
                            if (i.New == null && i.Final == null) {
                                WorkspaceItems.Remove(i);
                            } else {
                                objAction = WorkspaceAction.Updated;
                                i.UpdateProject(null);
                            }
                            lstUpdates.Add(GetUpdateEvent(objAction, i));
                        });
                        break;
                    case WatcherChangeTypes.Changed:
                        objWSItems.ForEach(i => {
                            i.UpdateProject(new MLTProject(u.Args.FullPath, VideoCache));
                            lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
                        });
                        break;
                    case WatcherChangeTypes.Created:
                        var objProject = new MLTProject(u.Args.FullPath, VideoCache);
                        var lstWsItems = WorkspaceItems.Where(i => i.New != null && i.New.Path == objProject.SourcePath).ToList();
                        if (lstWsItems.Count > 0) {
                            lstWsItems.ForEach(i => {
                                i.UpdateProject(objProject);
                                lstUpdates.Add(GetUpdateEvent(WorkspaceAction.Updated, i));
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

        public WorkspaceItem Get(Guid pItemID) {
            return WorkspaceItems.FirstOrDefault(i => i.ID.ToString().Equals(pItemID.ToString()));
        }
    }
}
