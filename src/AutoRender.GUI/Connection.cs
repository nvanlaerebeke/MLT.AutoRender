using Mitto.Connection.Websocket;
using Mitto.Messaging;
using System;
using System.Timers;

namespace AutoRender {

    internal class Connection {
        public readonly Mitto.Client Client;
        public bool IsConnected { get; private set; } = false;

        private Timer _objReconnectTimer;

        public event EventHandler Ready;

        public event EventHandler Disconnected;

        public Connection() {
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
            StartTimer();
        }

        private void Connect() {
            var objParams = new ClientParams() {
                Hostname = "192.168.0.126",
                Port = 6666,
                Secure = false
            };
            Client.ConnectAsync(objParams);
        }

        private void Connected(object sender, Mitto.Client e) {
            ClearTimer();
            IsConnected = true;
            Ready?.Invoke(this, new EventArgs());
        }

        private void ClientDisconnected(object sender, Mitto.Client e) {
            IsConnected = false;
            Disconnected?.Invoke(this, new EventArgs());
            StartTimer();
        }

        private void StartTimer() {
            ClearTimer();

            _objReconnectTimer = new Timer(5 * 1000);
            _objReconnectTimer.Elapsed += _objReconnectTimer_Elapsed;
            _objReconnectTimer.Enabled = true;
            _objReconnectTimer.Start();
        }

        private void ClearTimer() {
            try {
                if (_objReconnectTimer != null) {
                    _objReconnectTimer.Elapsed -= _objReconnectTimer_Elapsed;
                    _objReconnectTimer.Stop();
                    _objReconnectTimer.Dispose();
                    _objReconnectTimer = null;
                }
            } catch { }
        }

        private void _objReconnectTimer_Elapsed(object sender, ElapsedEventArgs e) {
            Connect();
        }
    }
}