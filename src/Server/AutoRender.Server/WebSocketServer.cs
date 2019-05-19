﻿using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using log4net;

namespace AutoRender.Server {

    internal class WebSocketServer {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public WebSocketServer() {
        }

        public void Start() { 
            new Mitto.Server().Start(new Mitto.Connection.Websocket.ServerParams(IPAddress.Any, 8080), (c) => {
                Log.Debug($"Client Connected");
            });
        }
    }
}