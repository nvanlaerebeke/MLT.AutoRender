using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using AutoRender.Data;
using AutoRender.Workspace;
using CrazyUtils;
using log4net;
using Mitto;
using Mitto.Connection.Websocket;

namespace AutoRender.Server {

    public class AutoRenderServer {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly WorkspaceMonitor WorkspaceMonitor;
        private readonly WebSocketServer Server;

        public AutoRenderServer() {
            SetupSystemNetWebSocket();
            //SetupWebSocketSharp();
            Cleanup();
            Server = new WebSocketServer();
            WorkspaceMonitor = new WorkspaceMonitor(WorkspaceFactory.Get());
        }

        /*private void SetupWebSocketSharp() {
            Config.Initialize(
                new Config.ConfigParams() {
                    ConnectionProvider = new Mitto.Connection.WebsocketSharp.WebSocketConnectionProvider() {
                        ServerConfig = new Mitto.Connection.WebsocketSharp.ServerParams() {
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
        }*/

        private void SetupSystemNetWebSocket() {
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