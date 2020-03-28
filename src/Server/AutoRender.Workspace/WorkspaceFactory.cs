using System.Reflection;
using log4net;

namespace AutoRender.Workspace {

    public static class WorkspaceFactory {
        private static readonly log4net.ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Workspace Workspace;

        public static Workspace Get() {
            if (Workspace == null) {
                Workspace = new Workspace();
                Workspace.ReLoad();
            }
            return Workspace;
        }
    }
}