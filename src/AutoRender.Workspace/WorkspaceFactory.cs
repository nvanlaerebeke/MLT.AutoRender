using System.Collections.Concurrent;

namespace AutoRender.Workspace {

    public static class WorkspaceFactory {
        private static ConcurrentDictionary<string, WorkspaceContainer> _dicWorkspaces = new ConcurrentDictionary<string, WorkspaceContainer>();

        public static WorkspaceContainer Get(string pPath) {
            if (_dicWorkspaces.ContainsKey(pPath)) {
                if (_dicWorkspaces.TryGetValue(pPath, out WorkspaceContainer objWorkspace)) {
                    return objWorkspace;
                }
            }

            var obj = new WorkspaceContainer(pPath);
            _dicWorkspaces.TryAdd(pPath, obj);
            Video.VideoFactory.Setup();
            return obj;
        }
    }
}