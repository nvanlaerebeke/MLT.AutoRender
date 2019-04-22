using AutoRender.Subscription.Messaging.Request;
using AutoRender.Subscription.Messaging.UnSubscribe;
using ILogging;
using Logging;
using Mitto.IMessaging;
using Mitto.IRouting;
using Mitto.Messaging.Response;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace AutoRender.Subscription.Messaging.Handlers {

    public class WorkspaceUpdatedHandler :
        ISubscriptionHandler<
            WorkspaceUpdatedSubscribe,
            WorkspaceUpdatedUnSubscribe,
            SendWorkspaceUpdatedRequest
        > {
        private readonly ILog Log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ConcurrentDictionary<string, IRouter> _dicClients = new ConcurrentDictionary<string, IRouter>();

        public bool Notify(SendWorkspaceUpdatedRequest pNotifyMessage) {
            return Notify(null, pNotifyMessage);
        }

        public bool Notify(IRouter pSender, SendWorkspaceUpdatedRequest pNotifyMessage) {
            _dicClients.Select(r => r.Value).ToList().ForEach((r) => {
                if (pSender == null || !r.ConnectionID.Equals(pSender.ConnectionID)) {
                    try {
                        MessagingFactory.Processor.Request<ACKResponse>(r, pNotifyMessage, (a => {
                            Log.Debug($"Notified {r.ConnectionID} about WorkspaceUpdated");
                        }));
                    } catch { }
                }
            });
            return true;
        }

        public bool Sub(IRouter pClient, WorkspaceUpdatedSubscribe pMessage) {
            if (!_dicClients.ContainsKey(pClient.ConnectionID)) {
                if (!_dicClients.TryAdd(pClient.ConnectionID, pClient)) {
                    return false;
                }
            }
            return true;
        }

        public bool UnSub(IRouter pClient, WorkspaceUpdatedUnSubscribe pMessage) {
            if (_dicClients.ContainsKey(pClient.ConnectionID)) {
                if (!_dicClients.TryRemove(pClient.ConnectionID, out _)) {
                    Log.Error($"Failed to remove {pClient.ConnectionID}, leaking memory");
                }
            }
            return true;
        }
    }
}