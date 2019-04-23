using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using ILogging;
using Logging;
using Mitto.IConnection;

namespace AutoRender.Server {

    internal class WebSocketServer {
        private readonly ILog Log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ConcurrentDictionary<string, IConnection> _lstClients = new ConcurrentDictionary<string, IConnection>();

        public WebSocketServer() {
        }

        public void Start() {
            var objServer = ConnectionFactory.CreateServer();
            objServer.Start(new Mitto.Connection.Websocket.ServerParams(IPAddress.Any, 6666), (c) => {
                Log.Debug($"Client {c.ID} Connected");
                c.Disconnected += Disconnected;
                _lstClients.TryAdd(c.ID, c);
            });
        }

        private void Disconnected(object sender, IConnection c) {
            c.Disconnected -= Disconnected;
            if (_lstClients.ContainsKey(c.ID)) {
                _lstClients.TryRemove(c.ID, out _);
            }
            Log.Debug($"Client {c.ID} Disconnected");
        }
    }
}