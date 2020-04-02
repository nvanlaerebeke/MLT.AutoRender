using System;
using System.Collections.Generic;
using System.Reflection;
using AutoRender.Data;
using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Workspace;
using log4net;
using Mitto.IMessaging;

namespace AutoRender.Server.Services {

    internal class WorkspaceMonitorService : Service {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Workspace.Workspace Workspace;

        public WorkspaceMonitorService() {
        }

        public override void Start() {
            try {
                Workspace = WorkspaceFactory.Get();
                Workspace.Updated += Workspace_Updated;
                Workspace.Reloaded += Workspace_Reloaded;
            } catch (Exception ex) {
                Log.Error("Failed to set up the workspace monitor, clients will not receive updates");
                Log.Error(ex);
            }
        }

        public override void Stop() {
            Workspace.Updated -= Workspace_Updated;
            Workspace.Reloaded -= Workspace_Reloaded;
        }

        private void Workspace_Reloaded(object sender, EventArgs e) {
            Log.Debug($"Workspace was reloaded, notifying clients...");
            _ = MessagingFactory.Provider.GetSubscriptionHandler<WorkspaceUpdatedHandler>().NotifyReload();
        }

        private void Workspace_Updated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            Log.Debug($"Workspace was updated, notifying clients...");
            _ = MessagingFactory.Provider.GetSubscriptionHandler<WorkspaceUpdatedHandler>().NotifyAll(
                new Subscription.Messaging.Request.SendWorkspaceUpdatedRequest(e)
            );
        }
    }
}