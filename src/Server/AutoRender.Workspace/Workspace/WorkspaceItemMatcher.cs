using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoRender.Workspace.Monitor;

namespace AutoRender.Workspace {

    /// <summary>
    /// ToDo: do all matching in the app here and make this an instance so the Workspace uses this class and can pass it's VideoInfoProvider
    /// </summary>
    internal static class WorkspaceItemMatcher {

        /// <summary>
        /// Find a match based on a list of workspace items
        /// </summary>
        /// <param name="pItems"></param>
        /// <param name="pType"></param>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static WorkspaceItem FindMatch(List<WorkspaceItem> pItems, WorkspaceType pType, string pPath) {
            switch (pType) {
                case WorkspaceType.Final:
                    var r1 = pItems.Where(w =>
                        (
                            w.Final != null &&
                            w.Final.Path.Equals(pPath)
                        ) ||
                        (
                            w.Project != null &&
                            w.Project.TargetPath.Equals(pPath)
                        )
                    ).FirstOrDefault();
                    if (r1 != null) {
                        return r1;
                    }
                    return pItems.Where(w =>
                        (
                            w.Final != null &&
                            w.Final.Path.StartsWith(pPath)
                        ) ||
                        (
                            w.Project != null &&
                            w.Project.TargetPath.StartsWith(pPath)
                        )
                    ).FirstOrDefault();

                case WorkspaceType.New:
                    return pItems.Where(w =>
                        (
                            w.New != null &&
                            w.New.Path.Equals(pPath)
                        ) ||
                        (
                            w.Project != null &&
                            w.Project.SourcePath.Equals(pPath)
                        )
                    ).FirstOrDefault();

                case WorkspaceType.Project:
                    return pItems.Where(w =>
                        (
                            w.Project != null &&
                            w.Project.FullPath.Equals(pPath)
                        ) ||
                        (
                            w.New != null &&
                            Path.GetFileName(w.New.Path).Equals(Path.ChangeExtension(Path.GetFileName(pPath), ".mp4"))
                        ) ||
                        (
                            w.Final != null &&
                            Path.GetFileName(w.Final.Path).Equals(new MLT.MLTProject(pPath, new Video.VideoInfoProvider()).TargetName)
                        )
                    ).FirstOrDefault();
            }
            return null;
        }
    }
}