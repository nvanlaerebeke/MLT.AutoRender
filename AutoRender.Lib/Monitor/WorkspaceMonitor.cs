using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutoRender.Lib.Monitor {
    public class WorkspaceMonitor {
        public event WorkspaceItemUpdated Updated;

        #region private Fields
        ProjectMonitor _objProjectMonitor;
        FinalMonitor _objFinalMonitor;
        NewMonitor _objNewMonitor;

        public List<WorkspaceItem> WorkspaceItems;
        #endregion
        public WorkspaceMonitor() {
            WorkspaceItems = new List<WorkspaceItem>();

            _objProjectMonitor = new ProjectMonitor();
            _objFinalMonitor = new FinalMonitor();
            _objNewMonitor = new NewMonitor();

            _objProjectMonitor.Changed += _objProjectMonitor_Changed;
            _objFinalMonitor.Changed += _objFinalMonitor_Changed;
            _objNewMonitor.Changed += _objNewMonitor_Changed;

            Load();
            WorkspaceItem.Updated += (sender, e) => { Updated?.Invoke(sender, e); };

            _objProjectMonitor.Start();
            _objNewMonitor.Start();
            _objFinalMonitor.Start();
        }

        internal void Reload() {
            WorkspaceItems.RemoveAll(i =>
                                     i.Project == null ||
                                     !(
                                         i.Project.Status == ProjectStatus.Busy ||
                                         i.Project.Status == ProjectStatus.Queued
                                    )
                                    );
            Load();
        }

        void _objFinalMonitor_Changed(List<FSEventInfo> pEvents) {
            Thread objThread = new Thread(() => {
                bool blnRequireReload = false;
                List<WorkspaceUpdatedEventArgs> lstChanges = new List<WorkspaceUpdatedEventArgs>();

                pEvents.ForEach(e => {
                    switch (e.Args.ChangeType) {
                        case System.IO.WatcherChangeTypes.Deleted:
                            lock (WorkspaceItems) {
                                lock (WorkspaceItems) {
                                    WorkspaceItems.Where(i => i.Project != null && i.Project.TargetPath == e.Args.FullPath).ToList().ForEach(i => {
                                        i.Project.Reload();
                                        i.UpdateFinal(null);
                                        lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Updated));
                                    });
                                    WorkspaceItems.Where(i => i.Project == null && i.Final != null && i.Final.Path == e.Args.FullPath).ToList().ForEach(i => {
                                        lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Deleted));
                                        WorkspaceItems.Remove(i);
                                    });
                                }
                            }
                            break;
                        case System.IO.WatcherChangeTypes.Renamed: // -- not yet done
                        case System.IO.WatcherChangeTypes.Created:
                            //Do not update when there is a running task for that file, it's no use doing that
                            var lstItems = WorkspaceItems.Where(i => i.Project != null && i.Project.TargetPath == e.Args.FullPath).ToList();
                            if (lstItems.Count == 0) {
                                blnRequireReload = true;
                            } else {
                                lstItems.ForEach(i => {
                                    if (i.Project != null && i.Project.Status != ProjectStatus.Busy) {
                                        blnRequireReload = true;
                                    }
                                });
                            }
                            break;
                        case System.IO.WatcherChangeTypes.Changed:
                            WorkspaceItems.Where(i =>
                                (
                                    i.Project != null &&
                                    i.Project.TargetPath == e.Args.FullPath
                                ) || (
                                   i.Final != null && i.Final.Path == e.Args.FullPath
                                )
                            ).ToList().ForEach(i => {

                                lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Updated));
                            });
                            break;
                    }
                });
                if (blnRequireReload) {
                    Load();
                }
                if (lstChanges.Count > 0) {
                    Updated?.Invoke(this, lstChanges);
                }
            }) {
                IsBackground = true
            };
            objThread.Start();
        }



        void _objProjectMonitor_Changed(List<FSEventInfo> pEvents) {
            Thread objThead = new Thread(() => {
                bool blnRequireReload = false;
                List<WorkspaceUpdatedEventArgs> lstChanges = new List<WorkspaceUpdatedEventArgs>();

                pEvents.ForEach(e => {
                    switch (e.Args.ChangeType) {
                        case System.IO.WatcherChangeTypes.Deleted:
                            lock (WorkspaceItems) {
                                //get items to delete, when nothing else (new/final) is set, remove it from the collection, else it's an update
                                var objItems = WorkspaceItems.Where(i =>
                                    i.Project != null &&
                                    i.Project.FullPath == e.Args.FullPath &&
                                    i.Final == null &&
                                    i.New == null
                                ).ToList();
                                foreach (var objItem in objItems) {
                                    if (objItem.New == null && objItem.Final == null) {
                                        WorkspaceItems.Remove(objItem);
                                        lstChanges.Add(new WorkspaceUpdatedEventArgs(objItem, WorkspaceAction.Deleted));
                                    }
                                }
                            }
                            break;
                        case System.IO.WatcherChangeTypes.Created:
                            lock (WorkspaceItems) {
                                var objProject = new Project(e.Args.FullPath);
                                List<WorkspaceItem> lstItemsToRemove = new List<WorkspaceItem>();
                                if (objProject != null) {
                                    lstItemsToRemove = WorkspaceItems.Where(i => i.New != null && i.New.Path == objProject.SourcePath).ToList();
                                }
                                if (lstItemsToRemove.Count > 0) {
                                    lstItemsToRemove.ForEach(i => {
                                        i.UpdateProject(objProject);
                                        lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Updated));
                                    });
                                } else {
                                    var objWSItem = new WorkspaceItem(objProject, VideoInfo.Get(objProject.Config.SourceFile), VideoInfo.Get(objProject.Config.TargetPath));
                                    lstChanges.Add(new WorkspaceUpdatedEventArgs(objWSItem, WorkspaceAction.New));
                                    WorkspaceItems.Add(objWSItem);
                                }
                            }
                            break;
                        case System.IO.WatcherChangeTypes.Renamed:
                        //ToDo implement remove of the old name
                        case System.IO.WatcherChangeTypes.Changed:
                            blnRequireReload = true;
                            break;
                    }
                });
                if (blnRequireReload) {
                    Load();
                }
                if (lstChanges.Count > 0) {
                    Updated?.Invoke(this, lstChanges);
                }
            }) {
                IsBackground = true
            };
            objThead.Start();
        }

        void _objNewMonitor_Changed(List<FSEventInfo> pEvents) {
            Thread objThread = new Thread(() => {

                bool blnRequireReload = false;
                List<WorkspaceUpdatedEventArgs> lstChanges = new List<WorkspaceUpdatedEventArgs>();

                pEvents.ForEach(e => {
                    switch (e.Args.ChangeType) {
                        case System.IO.WatcherChangeTypes.Changed:
                        case System.IO.WatcherChangeTypes.Created:
                            var wsl = WorkspaceItems.Where(i => (i.Project != null && i.Project.TargetPath.Equals(e.Args.FullPath)) || (i.New != null && i.New.Path.Equals(e.Args.FullPath))).ToList();
                            wsl.ForEach(i => {
                                i.UpdateNew(VideoInfo.Get(e.Args.FullPath));
                                lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Updated));
                            });
                            if (wsl.Count == 0) { // -- new file not linked to any project
                                var objItem = new WorkspaceItem(null, VideoInfo.Get(e.Args.FullPath), null);
                                lock (WorkspaceItems) {
                                    WorkspaceItems.Add(objItem);
                                }
                                lstChanges.Add(new WorkspaceUpdatedEventArgs(objItem, WorkspaceAction.New));
                            }
                            break;
                        case System.IO.WatcherChangeTypes.Deleted:
                            var wsl2 = WorkspaceItems.Where(i =>
                                                            (i.Project != null && i.Project.TargetPath.Equals(e.Args.FullPath)) ||
                                                            (i.New != null && i.New.Path.Equals(e.Args.FullPath))
                                                           ).ToList();
                            wsl2.ForEach(i => {
                                lock (WorkspaceItems) {
                                    WorkspaceItems.Remove(i);
                                }
                                lstChanges.Add(new WorkspaceUpdatedEventArgs(i, WorkspaceAction.Deleted));
                            });
                            break;
                        case System.IO.WatcherChangeTypes.Renamed:
                            blnRequireReload = true;
                            break;
                    }

                });
                if (blnRequireReload) {
                    Load();
                }
                if (lstChanges.Count > 0) {
                    Updated?.Invoke(this, lstChanges);
                }
            }) {
                IsBackground = true
            };
            objThread.Start();
        }

        private void Load() {
            List<WorkspaceUpdatedEventArgs> lstChanges = new List<WorkspaceUpdatedEventArgs>();

            lock (WorkspaceItems) {
                List<FileInfo> lstNew = new List<FileInfo>();
                List<FileInfo> lstFinal = new List<FileInfo>();
                List<FileInfo> lstProjects = new List<FileInfo>();

                var arrNew = Directory.GetFiles(Settings.NewDirectory, "*.mp4").OrderBy(p => p);
                foreach (string strPath in arrNew) {
                    lstNew.Add(new FileInfo(strPath));
                }

                var arrFinal = Directory.GetFiles(Settings.FinalDirectory, "*.mp4").OrderBy(p => p);
                foreach (string strPath in arrFinal) {
                    lstFinal.Add(new FileInfo(strPath));
                }

                var arrProjects = Directory.GetFiles(Settings.ProjectDirectory, "*.mlt").OrderBy(p => p);
                foreach (string strPath in arrProjects) {
                    lstProjects.Add(new FileInfo(strPath));
                }

                //Do the projects first as they can reference a file that isn't the same name as the project
                foreach (FileInfo objP in lstProjects) {
                    //if we already have the wsi with this project, use that one
                    var objWsItem = WorkspaceItems.Where(i => i.Project != null && i.Project.FullPath.Equals(objP.FullName)).FirstOrDefault();
                    Project objProject = (objWsItem == null) ? new Project(objP.FullName) : objWsItem.Project;
                    VideoInfo objNew = null;
                    VideoInfo objFinal = null;

                    foreach (FileInfo obj in lstNew) {
                        if (obj.Name == Path.GetFileName(objProject.SourcePath)) {
                            objNew = VideoInfo.Get(obj.FullName);
                            break;
                        }
                    }

                    foreach (FileInfo obj in lstFinal) {
                        if (obj.Name == objProject.TargetName) {
                            objFinal = VideoInfo.Get(obj.FullName);
                            break;
                        }
                    }

                    if (objWsItem == null) {
                        objWsItem = new WorkspaceItem(objProject, objNew, objFinal);
                        WorkspaceItems.Add(objWsItem);
                        lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.New));
                    } else {
                        if (
                            objWsItem.UpdateNew(objNew) ||
                            objWsItem.UpdateFinal(objFinal)
                        ) {
                            lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.Updated));
                        }
                    }
                }

                foreach (FileInfo objNew in lstNew) {
                    if (WorkspaceItems.Any(i =>
                                           (i.New != null && objNew.Name.Equals(i.New.FileName)) ||
                                           (i.Project != null && objNew.Name.Equals(Path.GetFileName(i.Project.SourcePath)))
                    )) { continue; }

                    VideoInfo objNewVideoInfo = VideoInfo.Get(objNew.FullName);
                    VideoInfo objFinal = null;

                    foreach (FileInfo obj in lstFinal) {
                        if (objNew.Name == obj.Name) {
                            objFinal = VideoInfo.Get(obj.FullName);
                            lstFinal.Remove(obj);
                            break;
                        }
                    }
                    var objWsItem = WorkspaceItems.Where(i => i.New != null && i.New.Path == objNew.FullName).FirstOrDefault();
                    if (objWsItem == null) {
                        objWsItem = new WorkspaceItem(null, objNewVideoInfo, objFinal);
                        WorkspaceItems.Add(objWsItem);
                        lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.New));
                    } else {
                        if (
                            objWsItem.UpdateNew(objNewVideoInfo) ||
                            objWsItem.UpdateProject(null) ||
                            objWsItem.UpdateFinal(objFinal)
                        ) {
                            lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.Updated));
                        }
                    }
                }

                foreach (FileInfo objFinal in lstFinal) {
                    if (WorkspaceItems.Any(i => i.Final != null && i.Final.FileName.Equals(objFinal.Name))) { continue; }
                    VideoInfo objFinalVideoInfo = VideoInfo.Get(objFinal.FullName);

                    var objWsItem = WorkspaceItems.FirstOrDefault(i => i.Final != null && i.Final.Path == objFinal.FullName);
                    if (objWsItem == null) {
                        objWsItem = new WorkspaceItem(null, null, VideoInfo.Get(objFinal.FullName));
                        WorkspaceItems.Add(objWsItem);
                        lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.New));
                    } else {
                        if (
                            objWsItem.UpdateNew(null) ||
                            objWsItem.UpdateProject(null) ||
                            objWsItem.UpdateFinal(objFinalVideoInfo)
                        ) {
                            lstChanges.Add(new WorkspaceUpdatedEventArgs(objWsItem, WorkspaceAction.Updated));
                        }
                    }
                }
            }
            Cleanup();
            Updated?.Invoke(null, lstChanges);
        }

        /// <summary>
        /// Deletes all temp directories not linked to a project
        /// </summary>
        private void Cleanup() {
            Directory.GetDirectories(Settings.TempDirectory).ToList().ForEach(d => {
                if (!WorkspaceItems.Any(i => i.Project != null && i.Project.ID.ToString().Equals(Path.GetFileName(d)))) {
                    Directory.Delete(d, true);
                }
            });
        }
    }
}