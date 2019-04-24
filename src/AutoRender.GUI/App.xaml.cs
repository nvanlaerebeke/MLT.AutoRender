using AutoRender.Logging;
using CrazyUtils;
using log4net;
using Mitto;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            using (new SingleInstance(1000)) { //1000ms timeout on global lock
                Logger.init(
                    log4net.Core.Level.Debug,
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AutoRender.log")
                );

                Config.Initialize(
                    new Config.ConfigParams() {
                        Logger = new MittoLogger(LogManager.GetLogger(typeof(Client)))
                    }
                );

                //show the main window
                MainWindow objMain = new MainWindow();
                objMain.Show();
            }
        }
    }
}