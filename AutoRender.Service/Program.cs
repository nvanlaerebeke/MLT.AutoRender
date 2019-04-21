using System;
using System.Threading;

namespace AutoRender.Service {
    static class Program {
        static ManualResetEvent _quit = new ManualResetEvent(false);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
            if (true) {
                Thread objThread = new Thread(() => {
                    AutoRender.Start();
                });
                objThread.IsBackground = true;
                objThread.Start();

                Console.CancelKeyPress += Console_CancelKeyPress;
                _quit.WaitOne();
            }/* else {
                /*ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] {
                    new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }*/

        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            _quit.Set();
        }
    }
}
