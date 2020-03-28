namespace AutoRender.Server {

    internal class WebSocketServer {
        private readonly Mitto.Server _Server;

        public WebSocketServer() {
            _Server = new Mitto.Server();
        }

        public void Start() {
            _Server.Start(null, (c) => { });
        }

        public void Stop() {
            _Server.Stop();
        }
    }
}