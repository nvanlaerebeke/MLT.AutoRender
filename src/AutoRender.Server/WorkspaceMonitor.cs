using AutoRender.Subscription.Messaging.Handlers;
using AutoRender.Workspace;
using ILogging;
using Logging;
using Mitto.IMessaging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoRender.Server {

    internal class WorkspaceMonitor {
        private readonly ILog Log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly WorkspaceContainer Workspace;

        public WorkspaceMonitor(WorkspaceContainer pWorkspace) {
            Workspace = pWorkspace;
        }

        public void Start() {
            Workspace.Updated += Workspace_Updated; // -- start listening to workspace update events so we can send out the events
        }

        private void Workspace_Updated(object sender, List<WorkspaceUpdatedEventArgs> e) {
            Task.Run(() => {
                Log.Debug($"Workspace was updated, notifying clients...");
                MessagingFactory.Provider.GetSubscriptionHandler<WorkspaceUpdatedHandler>().Notify(
                    new Subscription.Messaging.Request.SendWorkspaceUpdatedRequest()
                );
            });
        }
    }
}