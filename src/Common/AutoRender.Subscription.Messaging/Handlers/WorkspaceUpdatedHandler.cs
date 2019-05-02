using AutoRender.Subscription.Messaging.Request;
using AutoRender.Subscription.Messaging.UnSubscribe;
using log4net;
using Mitto.IMessaging;
using System;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, IClient> _dicClients = new ConcurrentDictionary<string, IClient>();

        public bool NotifyAll(SendWorkspaceUpdatedRequest pNotifyMessage) {
            return Notify(null, pNotifyMessage);
        }

        public bool Notify(IClient pSender, SendWorkspaceUpdatedRequest pNotifyMessage) {
            _dicClients.Select(c => c.Value).ToList().ForEach((c) => {
                if (pSender == null || !c.ID.Equals(pSender.ID)) {
                    try {
                        c.Notify(pNotifyMessage);
                    } catch (Exception ex) {
                        Log.Error($"Failed to send SendWorkspaceUpdatedRequest to {c.ID}: {ex.Message}");
                    }
                }
            });
            return true;
        }

        public bool Sub(IClient pClient, WorkspaceUpdatedSubscribe pMessage) {
            if (!_dicClients.ContainsKey(pClient.ID)) {
                if (!_dicClients.TryAdd(pClient.ID, pClient)) {
                    return false;
                }
                pClient.Disconnected += Client_Disconnected;
            }
            return true;
        }

        public bool UnSub(IClient pClient, WorkspaceUpdatedUnSubscribe pMessage) {
            if (_dicClients.ContainsKey(pClient.ID)) {
                if (!_dicClients.TryRemove(pClient.ID, out _)) {
                    Log.Error($"Failed to remove {pClient.ID}, leaking memory");
                } else {
                    pClient.Disconnected -= Client_Disconnected;
                }
            }
            return true;
        }

        private void Client_Disconnected(object sender, IClient e) {
            UnSub(e, new WorkspaceUpdatedUnSubscribe());
        }
    }
}