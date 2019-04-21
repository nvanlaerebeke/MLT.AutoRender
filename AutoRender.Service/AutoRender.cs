using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AutoRender.Lib;

namespace AutoRender.Service {
    public static class AutoRender {
        public static void Start() {
            using (new Lib.Helpers.SingleInstanceHelper(1000)) { //1000ms timeout on global lock
                Logger.init();

                //verify settings
                var strFFProbePath = Settings.FfprobePath;
                var strMeltPath = Settings.MeltPath;

                if (!File.Exists(strFFProbePath)) { throw new Exception("FFProbe not found: " + strFFProbePath); }
                if (!File.Exists(strMeltPath)) { throw new Exception("Melt not found: " + strMeltPath); }
                LoadEnv();


                Cleanup();
                Lib.Helpers.ErrorHelper.Init();

                Workspace.StartMonitoring(); // -- construct the workspace
                Websocket.WebsocketServer.Start(); // -- start the websocket server
                Workspace.Updated += Workspace_Updated; // -- start listening to workspace update events so we can send out the events
            }
        }

        private static Messaging.MessageProvider _objProvider;

        private static void Workspace_Updated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            ThreadPool.QueueUserWorkItem((state) => {
                //Create a provider and keep it for caching, loading types is intensive
                if (_objProvider == null) { _objProvider = new Messaging.MessageProvider(); }

                //get all workspaceupdate messages
                List<Messaging.WorkspaceItemUpdate> lstUpdates = new List<Messaging.WorkspaceItemUpdate>();
                e.ForEach(u => lstUpdates.Add(new Messaging.WorkspaceItemUpdate(u.WorkspaceItem, u.Action)));

                //create and send message
                var objMessage = new Messaging.Notification.WorkspaceUpdated(lstUpdates);
                _objProvider.GetSubscriptionHandler(objMessage).Notify(objMessage);
            });
        }

        private static void Cleanup() {
            if (System.IO.Directory.Exists(Settings.TempDirectory)) {
                var arrFiles = Directory.GetFiles(Settings.TempDirectory);
                try {
                    foreach (var strFile in arrFiles) {
                        File.Delete(strFile);
                    }
                } catch { }
            }
        }

        private static void LoadEnv() {
            Process objProcess = new Process();
            ProcessStartInfo objStartInfo = new ProcessStartInfo(Settings.SourcePath, Path.GetDirectoryName(Settings.SourcePath)) {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                WorkingDirectory = Settings.TempDirectory
            };

            objProcess.StartInfo = objStartInfo;
            objProcess.Start();
        }
    }
}