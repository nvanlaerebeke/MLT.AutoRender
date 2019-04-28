using System.Collections.Concurrent;

namespace AutoRender.Workspace {

    public static class WorkspaceFactory {
        private static readonly WorkspaceContainer WorkspaceContainer = new WorkspaceContainer();

        public static WorkspaceContainer Get() {
            return WorkspaceContainer;
        }
    }
}