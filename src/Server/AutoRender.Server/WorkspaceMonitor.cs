using AutoRender.Data;
using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Workspace;
using log4net;
using Mitto.IMessaging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoRender.Server {

    internal class WorkspaceMonitor {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Workspace.Workspace Workspace;

        public WorkspaceMonitor(Workspace.Workspace pWorkspace) {
            Workspace = pWorkspace;
        }

        public void Start() {
            Workspace.Updated += Workspace_Updated; // -- start listening to workspace update events so we can send out the events
        }

        private void Workspace_Updated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            Task.Run(() => {
                Log.Debug($"Workspace was updated, notifying clients...");
                MessagingFactory.Provider.GetSubscriptionHandler<WorkspaceUpdatedHandler>().NotifyAll(
                    new Subscription.Messaging.Request.SendWorkspaceUpdatedRequest(e)
                );
            });
        }
    }
}