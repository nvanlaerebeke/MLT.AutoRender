using System;
using System.Collections.Generic;
using System.IO;
using AutoRender.Video;
using AutoRender.Workspace.Monitor;

namespace AutoRender.Workspace {

    internal static class Updater {

        /// <summary>
        /// ToDo: Pass videoinfoprovider
        /// </summary>
        /// <param name="pContainer"></param>
        /// <param name="pUpdateType"></param>
        /// <param name="pEvents"></param>
        public static void Apply(WorkspaceContainer pContainer, VideoInfoProvider pVideoInfoProvider, WorkspaceType pUpdateType, List<FSEventInfo> pEvents) {
            pEvents.ForEach(e => {
                switch (e.Args.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        Update(pContainer, pVideoInfoProvider, pUpdateType, e);
                        break;

                    case WatcherChangeTypes.Deleted:
                        Delete(pContainer, pUpdateType, e);
                        break;

                    case WatcherChangeTypes.Renamed:
                        throw new NotImplementedException();
                        //Delete(pContainer, pUpdateType, e);
                        //Update(pContainer, pVideoInfoProvider, pUpdateType, e);
                        //break;
                }
            });
        }

        private static void Update(WorkspaceContainer pContainer, VideoInfoProvider pVideoInfoProvider, WorkspaceType pType, FSEventInfo pEvent) {
            var objWSItem = WorkspaceItemMatcher.FindMatch(pContainer.GetAll(), pType, pEvent.Args.FullPath);
            if (objWSItem != null) {
                switch (pType) {
                    case WorkspaceType.Final:
                        var path = pEvent.Args.FullPath;
                        if (pEvent.Args.ChangeType != WatcherChangeTypes.Deleted) {
                            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                                if (objWSItem.Project != null) {
                                    if (File.Exists(objWSItem.Project.TargetPath)) {
                                        path = objWSItem.Project.TargetPath;
                                    }
                                }
                            }
                        }
                        _ = objWSItem.UpdateFinal(
                            File.Exists(path) ? pVideoInfoProvider.Get(path) : null
                        );
                        break;

                    case WorkspaceType.New:
                        _ = objWSItem.UpdateNew(pVideoInfoProvider.Get(pEvent.Args.FullPath));
                        break;

                    case WorkspaceType.Project:
                        _ = objWSItem.UpdateProject(new MLT.MLTProject(pEvent.Args.FullPath, pVideoInfoProvider));
                        break;
                }
            } else {
                switch (pType) {
                    case WorkspaceType.Final:
                        pContainer.Add(new WorkspaceItem(null, null, pVideoInfoProvider.Get(pEvent.Args.FullPath)));
                        break;

                    case WorkspaceType.New:
                        pContainer.Add(new WorkspaceItem(null, pVideoInfoProvider.Get(pEvent.Args.FullPath), null));
                        break;

                    case WorkspaceType.Project:
                        pContainer.Add(new WorkspaceItem(new MLT.MLTProject(pEvent.Args.FullPath, pVideoInfoProvider), null, null));
                        break;
                }
            }
        }

        private static void Delete(WorkspaceContainer pContainer, WorkspaceType pType, FSEventInfo pEvent) {
            var objWSItem = WorkspaceItemMatcher.FindMatch(pContainer.GetAll(), pType, pEvent.Args.FullPath);
            if (objWSItem != null) {
                switch (pType) {
                    case WorkspaceType.Final:
                        if (objWSItem.New == null && objWSItem.Project == null) {
                            pContainer.Remove(objWSItem);
                        } else {
                            _ = objWSItem.UpdateFinal(null);
                        }
                        break;

                    case WorkspaceType.New:
                        if (objWSItem.Final == null && objWSItem.Project == null) {
                            pContainer.Remove(objWSItem);
                        } else {
                            _ = objWSItem.UpdateNew(null);
                        }
                        break;

                    case WorkspaceType.Project:
                        if (objWSItem.New == null && objWSItem.Final == null) {
                            pContainer.Remove(objWSItem);
                        } else {
                            _ = objWSItem.UpdateProject(null);
                        }
                        break;
                }
            }
        }
    }
}