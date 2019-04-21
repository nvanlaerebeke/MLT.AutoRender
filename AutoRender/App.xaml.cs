using System;
using System.Windows;
using System.IO;
using System.Threading;
using WebSocketMessaging;

namespace AutoRender {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            using (new CrazyUtils.SingleInstanceHelper(1000)) { //1000ms timeout on global lock
                //Init(); // -- initialize the application
                CrazyUtils.Logger.init();

                //show the main window
                MainWindow objMain = new MainWindow();
                objMain.Show();
            }
        }

        /*private static void Init() {
            Logger.init();

            //verify settings
            var strFFProbePath = Settings.FfprobePath;
            var strMeltPath = Settings.MeltPath;

            if (!File.Exists(strFFProbePath)) {
                MessageBox.Show("FFProbe not found: " + strFFProbePath);
            }
            if (!File.Exists(strMeltPath)) {
                MessageBox.Show("Melt not found: " + strMeltPath);
            }

            if (!Directory.Exists(Settings.NewDirectory)) { Directory.CreateDirectory(Settings.NewDirectory); }
            if (!Directory.Exists(Settings.LogDirectory)) { Directory.CreateDirectory(Settings.LogDirectory); }
            if (!Directory.Exists(Settings.FinalDirectory)) { Directory.CreateDirectory(Settings.FinalDirectory); }
            if (!Directory.Exists(Settings.ProjectDirectory)) { Directory.CreateDirectory(Settings.ProjectDirectory); }
            if (!Directory.Exists(Settings.TempDirectory)) { Directory.CreateDirectory(Settings.TempDirectory); }

            Cleanup();

            Helpers.ErrorHelper.Init();
            Helpers.MonitorHelper.Init();
            Melt.MeltJobScheduler.Init();
        }*/

        private static void Cleanup() {
           /* var arrFiles = Directory.GetFiles(Settings.TempDirectory);
            try {
                foreach(var strFile in arrFiles) {
                    File.Delete(strFile);
                }
            } catch (Exception) { }*/
        }
    }
}
