using Mitto;
using AutoRender.Workspace;
using AutoRender.Logging;
using log4net;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using AutoRender.Data;
using System;
using Mitto.Connection.Websocket;
using System.Net;
using CrazyUtils;

namespace AutoRender.Server {

    public class AutoRenderServer {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly WorkspaceMonitor WorkspaceMonitor;
        private readonly WebSocketServer Server;

        public AutoRenderServer() {
            Config.Initialize(
                new Config.ConfigParams() {
                    ConnectionProvider = new WebSocketConnectionProvider() {
                        ServerConfig = new ServerParams() {
                            IP = IPAddress.Any,
                            Port = 80,
                            Path = "/",
                            FragmentSize = 512,
                        }
                    },
                    //Logger = new MittoLogger(LogManager.GetLogger(typeof(Mitto.Server))),
                    Assemblies = new List<AssemblyName> {
                        new AssemblyName("AutoRender.Messaging"),
                        new AssemblyName("AutoRender.Messaging.Actions")
                    }
                }
            );
            Cleanup();
            Server = new WebSocketServer();
            WorkspaceMonitor = new WorkspaceMonitor(WorkspaceFactory.Get());
        }

        public void Start() {
            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                WorkspaceMonitor.Start();
                Server.Start();
            } else {
                using (new SingleInstance(1000)) { //1000ms timeout on global lock
                    WorkspaceMonitor.Start();
                    Server.Start();
                }
            }
        }

        private void Cleanup() {
            try {
                if (Directory.Exists(Settings.TempDirectory)) {
                    Directory.Delete(Settings.TempDirectory, true);
                }
            } catch (Exception ex) {
                Log.Error($"Failed to clean up {Settings.TempDirectory}: {ex.Message}");
            }
        }
    }
}