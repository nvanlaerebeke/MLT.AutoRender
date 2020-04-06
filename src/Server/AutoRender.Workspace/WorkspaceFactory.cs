using System.Reflection;
using AutoRender.Server.Config;
using log4net;

namespace AutoRender.Workspace {

    public static class WorkspaceFactory {
        private static readonly log4net.ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Workspace Workspace;

        public static Workspace Get() {
            if (Workspace == null) {
                Workspace = new Workspace(
                    Settings.NewDirectory,
                    Settings.FinalDirectory,
                    Settings.ProjectDirectory,
                    Settings.TempDirectory,
                    new Video.VideoInfoProvider()
                );
            }
            return Workspace;
        }
    }
}