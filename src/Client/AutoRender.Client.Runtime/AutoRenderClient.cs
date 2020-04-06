using AutoRender.Client.Config;
using AutoRender.Client.Connection;

namespace AutoRender.Client.Runtime {

    /// <summary>
    /// ToDo: this should do the application startup (including windows)
    ///       - not yet in use -
    ///
    /// </summary>
    public class AutoRenderClient {
        private readonly WorkspaceConnection WorkspaceConnection;

        public AutoRenderClient() {
            var objManager = new ConnectionManager(Settings.HostName, Settings.Port);
            WorkspaceConnection = new WorkspaceConnection(objManager, new Workspace.Workspace(objManager.Client));
        }

        public void Start() {
            WorkspaceConnection.Start();
        }
    }
}