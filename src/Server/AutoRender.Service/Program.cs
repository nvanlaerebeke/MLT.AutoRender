using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using AutoRender.Server;
using CrazyUtils;

namespace AutoRender.Service {

    internal static class Program {
        private static readonly ManualResetEvent _quit = new ManualResetEvent(false);
        private static AutoRenderServer AutoRender;

        private static void Main() {
            if (Debugger.IsAttached) {
                using (new SingleInstance(1000)) { //1000ms timeout on global lock
                    new Thread(() => {
                        AutoRender = new AutoRenderServer();
                        AutoRender.Start();
                    }) { IsBackground = true }.Start();
                    System.Console.CancelKeyPress += Console_CancelKeyPress;
                    _ = _quit.WaitOne();
                }
            } else {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new AutoRenderService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void Console_CancelKeyPress(object sender, System.ConsoleCancelEventArgs e) {
            _ = _quit.Set();
        }
    }
}