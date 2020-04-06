using System;
using System.Threading.Tasks;
using Mitto.Messaging;

namespace AutoRender.Client.Connection {

    public class Client {

        public event EventHandler<ConnectionStatus> StatusChanged;

        private readonly string HostName;
        private readonly int Port;
        private Mitto.Client _objClient;

        public ConnectionStatus Status {
            get {
                switch (_objClient.Status) {
                    case Mitto.IConnection.ConnectionState.Connecting:
                        return ConnectionStatus.Connecting;

                    case Mitto.IConnection.ConnectionState.Open:
                        return ConnectionStatus.Connected;

                    default:
                        return ConnectionStatus.Disconnected;
                }
            }
        }

        public Client(string pHostName, int pPort) {
            HostName = pHostName;
            Port = pPort;

            _objClient = new Mitto.Client();
            _objClient.Connected += Connected;
            _objClient.Disconnected += ClientDisconnected;
        }

        public void Request<T>(RequestMessage pMessage, Action<T> pCallBack) where T : ResponseMessage {
            if (_objClient != null) {
                _objClient.Request<T>(pMessage, pCallBack);
            }
        }

        public async Task<T> Request<T>(RequestMessage pMessage) where T : ResponseMessage {
            if (_objClient != null) {
                return await _objClient.RequestAsync<T>(pMessage);
            }
            return null;
        }

        public void Connect() {
            if (_objClient != null) {
                if (
                    _objClient.Status == Mitto.IConnection.ConnectionState.Open ||
                    _objClient.Status == Mitto.IConnection.ConnectionState.Connecting
                ) {
                    _objClient.Disconnect();
                }
                _objClient.Connected -= Connected;
                _objClient.Disconnected -= ClientDisconnected;
            }

            _objClient = new Mitto.Client();
            _objClient.Connected += Connected;
            _objClient.Disconnected += ClientDisconnected;

            StatusChanged?.Invoke(this, ConnectionStatus.Connecting);
            _objClient.ConnectAsync(new Mitto.Connection.Websocket.ClientParams() {
                HostName = HostName,
                Port = Port,
                Secure = false,
            });
        }

        private void Connected(object sender, Mitto.Client e) {
            StatusChanged?.Invoke(this, ConnectionStatus.Connected);
        }

        private void ClientDisconnected(object sender, Mitto.Client e) {
            StatusChanged?.Invoke(this, ConnectionStatus.Disconnected);
        }
    }
}