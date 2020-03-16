namespace AutoRender.Server {

    internal class WebSocketServer {

        public WebSocketServer() {
        }

        public void Start() {
            new Mitto.Server().Start(null, (c) => { });
        }
    }
}