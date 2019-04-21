using AutoRender.Messaging;
using System;
using System.Reflection;
using System.Timers;
using WebSocketMessaging;

namespace AutoRender {
    internal class Connection {
        private Client _objConnection;
        private Timer _objReconnectTimer;

        public event EventHandler Ready;
        public event EventHandler Disconnected;
        public Connection() { }
        public T Request<T>(RequestMessage pMessage) where T : ResponseMessage {
            if (_objConnection != null) {
                return _objConnection.Send<T>(pMessage);
            }
            return null;
        }

        public void Start() {
            StartTimer();
        }

        private void _objConnection_disconnected(Client pClient) {
            Disconnected?.Invoke(pClient, new EventArgs());
            StartTimer();
        }

        private void Connect() {
            if (_objConnection != null) {
                _objConnection.connected -= _objConnection_connected;
                _objConnection.disconnected -= _objConnection_disconnected;
                _objConnection.Close();
            }
            //_objConnection = new Client("192.168.0.112", 6666, new ClientInfo(Settings.LocationID, new Version("6.6.6.6"), Settings.LocationName, MessageFormat.Json), false, new Messaging.MessageProvider());
            _objConnection = new Client("test.crazyzone.be", 6666, new ClientInfo(Settings.LocationID, new Version("6.6.6.6"), Settings.LocationName, MessageFormat.Json), false, new Messaging.MessageProvider());
            //_objConnection = new Client("renderbox.crazyzone.be", 6666, new ClientInfo(Settings.LocationID, Assembly.GetExecutingAssembly().GetName().Version, Settings.LocationName, MessageFormat.Json), false, new Messaging.MessageProvider());
            //_objConnection = new Client(Settings.Server, 6666, new ClientInfo(Settings.LocationID, Assembly.GetExecutingAssembly().GetName().Version, Settings.LocationName, MessageFormat.Json), false, new Messaging.MessageProvider());

            _objConnection.connected += _objConnection_connected;
            _objConnection.disconnected += _objConnection_disconnected;
            _objConnection.ConnectAsync();
        }
        private void _objConnection_connected(Client pClient) {
            ClearTimer();
            Ready?.Invoke(this, new EventArgs());
        }

        private void StartTimer() {
            ClearTimer();

            _objReconnectTimer = new Timer(5 * 1000);
            _objReconnectTimer.Elapsed += _objReconnectTimer_Elapsed;
            _objReconnectTimer.Enabled = true;
            _objReconnectTimer.Start();
        }
        private void ClearTimer() {
            if (_objReconnectTimer != null) {
                _objReconnectTimer.Elapsed -= _objReconnectTimer_Elapsed;
                _objReconnectTimer.Stop();
                _objReconnectTimer.Dispose();
                _objReconnectTimer = null;
            }
        }
        private void _objReconnectTimer_Elapsed(object sender, ElapsedEventArgs e) {
            Connect();
        }
    }
}