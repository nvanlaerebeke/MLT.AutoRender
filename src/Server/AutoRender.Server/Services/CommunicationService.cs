using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Mitto;
using Mitto.Connection.Websocket;

namespace AutoRender.Server.Services {

    internal class CommunicationService : Service {
        private WebSocketServer Server;

        public CommunicationService() {
        }

        public override void Start() {
            try {
                ConfigMitto();
                Server = new WebSocketServer();
                Server.Start();
            } catch (Exception ex) {
                Log.Error("Failed to set up the websocket server, clients will not be able to connect");
                Log.Error(ex);
            }
        }

        public override void Stop() {
            try {
                Server.Stop();
            } catch (Exception ex) {
                Log.Error("Failed to stop the websocket server");
                Log.Error(ex);
            }
        }

        private void ConfigMitto() {
            Log.Debug("Config Mitto with websockets");
            Config.Initialize(
                new Config.ConfigParams() {
                    ConnectionProvider = new WebSocketConnectionProvider() {
                        ServerConfig = new ServerParams() {
                            IP = IPAddress.Any,
                            Port = 37697,
                            Path = "/",
                            FragmentSize = 512,
                        }
                    },
                    Assemblies = new List<AssemblyName> {
                        new AssemblyName("AutoRender.Messaging"),
                        new AssemblyName("AutoRender.Messaging.Actions")
                    }
                }
            );
        }
    }
}