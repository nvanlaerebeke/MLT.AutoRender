using System;
using System.Threading;
using AutoRender.Server;
using Logging;
using Logging.Log4Net;

namespace AutoRender.Service {

    internal static class Program {
        private static ManualResetEvent _quit = new ManualResetEvent(false);
        private static AutoRenderServer AutoRender;

        private static void Main() {
            LogFactory.Initialize(new LogProvider());

            AutoRender = new AutoRenderServer();
            AutoRender.Start();

            Console.CancelKeyPress += (s, e) => {
                _quit.Set();
            };
            _quit.WaitOne();
        }
    }
}