using CrazyUtils;
using Mitto;
using AutoRender.Workspace;
using AutoRender.Logging;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace AutoRender.Server {

    public class AutoRenderServer {
        private readonly WorkspaceMonitor WorkspaceMonitor;
        private readonly WebSocketServer Server;

        public AutoRenderServer() {
            /*var tmp = typeof(AutoRender.Messaging.Request.ReloadRequest);*/
            var tmp2 = typeof(AutoRender.Messaging.Action.Request.GetStatusRequestAction);
            Config.Initialize(
                new Config.ConfigParams() {
                    Logger = new MittoLogger(LogManager.GetLogger(typeof(Mitto.Server))),
                    Assemblies = new List<AssemblyName> {
                        new AssemblyName("AutoRender.Messaging"),
                        new AssemblyName("AutoRender.Messaging.Actions")
                    }
                }
            );
            Server = new WebSocketServer();
            WorkspaceMonitor = new WorkspaceMonitor(WorkspaceFactory.Get());
        }

        public void Start() {
            //using (new SingleInstance(1000)) { //1000ms timeout on global lock
                WorkspaceMonitor.Start();
                Server.Start();
            //}
        }
    }
}