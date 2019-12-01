using System.Net;
using System.Reflection;
using log4net;
using Mitto;
using Mitto.Connection.Websocket;

namespace AutoRender.Server {

    internal class WebSocketServer {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public WebSocketServer() {
        }

        public void Start() {
            /*Config.Initialize(new Config.ConfigParams() {
                ConnectionProvider = new WebSocketConnectionProvider() {
                    ServerConfig = new ServerParams() {
                        IP = IPAddress.Any,
                        Port = 80,
                        Path = "/",
                        FragmentSize = 512,
                    }
                }
            });*/
            new Mitto.Server().Start(null, (c) => {
                Log.Debug($"Client Connected");
            });
        }
    }
}