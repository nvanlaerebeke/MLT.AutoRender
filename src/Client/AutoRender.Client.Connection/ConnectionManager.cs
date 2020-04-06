using System;
using Mitto.Utilities;

namespace AutoRender.Client.Connection {

    public class ConnectionManager {

        public event EventHandler<ConnectionStatus> StatusChanged;

        public readonly Client Client;
        public ConnectionStatus Status = ConnectionStatus.Disconnected;

        private readonly Timer ReconnectTimer;

        public ConnectionManager(string pHostName, int pPort) {
            Client = new Client(pHostName, pPort);
            Client.StatusChanged += Connection_StatusChanged;

            ReconnectTimer = new Timer(5);
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
        }

        private void Connection_StatusChanged(object sender, ConnectionStatus e) {
            Status = e;
            StatusChanged?.Invoke(sender, e);
            if (e == ConnectionStatus.Disconnected) {
                ReconnectTimer.Start();
            }
        }

        public void Start() {
            if (Client.Status == ConnectionStatus.Disconnected) {
                Client.Connect();
            }
        }

        #region EventHandlers

        private void Connection_Ready(object sender, EventArgs e) {
            StatusChanged?.Invoke(this, ConnectionStatus.Connected);
        }

        private void ReconnectTimer_Elapsed(object sender, EventArgs e) {
            Start();
        }

        #endregion EventHandlers
    }
}