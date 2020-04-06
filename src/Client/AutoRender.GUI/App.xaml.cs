using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using AutoRender.Client.Config;
using AutoRender.Client.Connection;
using AutoRender.Client.Runtime;
using AutoRender.Client.Workspace;
using AutoRender.Logging;
using CrazyUtils;
using log4net;
using Mitto;
using Mitto.Connection.Websocket;

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
                        ConnectionProvider = new WebSocketConnectionProvider(),
                        Logger = new MittoLogger(LogManager.GetLogger(typeof(App))),
                        Assemblies = new List<AssemblyName>() {
                            new AssemblyName("AutoRender.Messaging"),
                            new AssemblyName("AutoRender.Messaging.Actions"),
                            new AssemblyName("AutoRender.Subscription.Messaging")
                        }
                    }
                );
                //show the main window
                var obj = new ConnectionManager(Settings.HostName, Settings.Port);
                new MainWindow(new WorkspaceConnection(obj, new Workspace(obj.Client))).Show();
            }
        }
    }
}