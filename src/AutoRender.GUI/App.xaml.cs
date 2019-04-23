using CrazyUtils;
using System.Windows;

namespace AutoRender {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            using (new SingleInstance(1000)) { //1000ms timeout on global lock
                //show the main window
                Mitto.Config.Initialize();
                MainWindow objMain = new MainWindow();
                objMain.Show();
            }
        }
    }
}