using AutoRender.Messaging;
using System.Net;
using WebSocketMessaging;

namespace AutoRender.Service.Websocket {
    public static class WebsocketServer {
        private static Server _objServer;
        public static void Start() {
            _objServer = new Server(IPAddress.Any, 6666, new Messaging.MessageProvider());
            _objServer.Start();
        }
    }
}
