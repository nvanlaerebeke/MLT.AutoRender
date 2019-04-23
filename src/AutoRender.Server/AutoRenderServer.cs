using CrazyUtils;
using Mitto;
using AutoRender.Workspace;

namespace AutoRender.Server {

    public class AutoRenderServer {
        private readonly WorkspaceMonitor WorkspaceMonitor;
        private readonly WebSocketServer Server;

        public AutoRenderServer() {
            Config.Initialize(new Config.ConfigParams() { });
            Server = new WebSocketServer();

            WorkspaceMonitor = new WorkspaceMonitor(WorkspaceFactory.Get(""));
        }

        public void Start() {
            using (new SingleInstance(1000)) { //1000ms timeout on global lock
                WorkspaceMonitor.Start();
                Server.Start();
            }
        }
    }
}