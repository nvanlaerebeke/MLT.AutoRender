using System;
using System.Threading;
using AutoRender.Server;
using AutoRender.Logging;
using System.IO;
using System.Reflection;

namespace AutoRender.Service {

    internal static class Program {
        private static ManualResetEvent _quit = new ManualResetEvent(false);
        private static AutoRenderServer AutoRender;

        private static void Main() {
            //LogFactory.Initialize(new LogProvider());
            Logger.init(
                log4net.Core.Level.Debug,
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AutoRender.log")
            );

            AutoRender = new AutoRenderServer();
            AutoRender.Start();

            Console.CancelKeyPress += (s, e) => {
                _quit.Set();
            };
            _quit.WaitOne();
        }
    }
}