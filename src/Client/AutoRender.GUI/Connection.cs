using System;
using Mitto.Messaging;
using Mitto.Utilities;

namespace AutoRender {

    internal class Connection {
        private readonly Timer ReconnectTimer;
        private Mitto.Client Client;

        public event EventHandler Ready;

        public event EventHandler Disconnected;

        public bool IsConnected { get; private set; } = false;

        public Connection() {
            ReconnectTimer = new Timer(5);
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
        }

        public void Request<T>(RequestMessage pMessage, Action<T> pCallBack) where T : ResponseMessage {
            if (Client != null) {
                Client.Request<T>(pMessage, pCallBack);
            }
        }

        public void Start() {
            Connect();
        }

        private void Connect() {
            if (Client != null) {
                Client.Connected += Connected;
                Client.Disconnected += ClientDisconnected;
                Client.Disconnect();
            }

            Client = new Mitto.Client();
            Client.Connected += Connected;
            Client.Disconnected += ClientDisconnected;

            Client.ConnectAsync(new Mitto.Connection.Websocket.ClientParams() {
                HostName = "localhost",
                Port = 37697,
                Secure = false,
            });
        }

        private void Connected(object sender, Mitto.Client e) {
            ReconnectTimer.Stop();
            IsConnected = true;
            Ready?.Invoke(this, new EventArgs());
        }

        private void ClientDisconnected(object sender, Mitto.Client e) {
            IsConnected = false;
            Disconnected?.Invoke(this, new EventArgs());
            ReconnectTimer.Start();
        }

        private void ReconnectTimer_Elapsed(object sender, EventArgs e) {
            Connect();
        }
    }
}