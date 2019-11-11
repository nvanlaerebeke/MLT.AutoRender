using Mitto.Connection.Websocket;
using Mitto.Messaging;
using Mitto.Utilities;
using System;

namespace AutoRender {

    internal class Connection {
        private readonly Timer ReconnectTimer;
        private readonly Mitto.Client Client;

        public event EventHandler Ready;

        public event EventHandler Disconnected;

        public bool IsConnected { get; private set; } = false;

        public Connection() {
            ReconnectTimer = new Timer(5);
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;

            Client = new Mitto.Client();
            Client.Connected += Connected;
            Client.Disconnected += ClientDisconnected;
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
            var objParams = new ClientParams() {
                Hostname = "localhost",
                Port = 6666,
                Secure = false
            };
            Client.ConnectAsync(objParams);
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