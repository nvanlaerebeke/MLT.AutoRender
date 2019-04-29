using AutoRender.Subscription.Messaging.Request;
using AutoRender.Subscription.Messaging.UnSubscribe;
using log4net;
using Mitto.IMessaging;
using Mitto.IRouting;
using Mitto.Messaging.Response;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace AutoRender.Subscription.Messaging.Handlers {
    /// <summary>
    /// ToDo: Add disconnected event so the Router list can be cleaned up
    /// </summary>
    public class WorkspaceUpdatedHandler :
        ISubscriptionHandler<
            WorkspaceUpdatedSubscribe,
            WorkspaceUpdatedUnSubscribe,
            SendWorkspaceUpdatedRequest
        > {
        private readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ConcurrentDictionary<string, IRouter> _dicClients = new ConcurrentDictionary<string, IRouter>();

        public bool NotifyAll(SendWorkspaceUpdatedRequest pNotifyMessage) {
            return Notify(null, pNotifyMessage);
        }

        public bool Notify(IRouter pSender, SendWorkspaceUpdatedRequest pNotifyMessage) {
            _dicClients.Select(r => r.Value).ToList().ForEach((r) => {
                if (pSender == null || !r.ConnectionID.Equals(pSender.ConnectionID)) {
                    try {
                        MessagingFactory.Processor.Request<ACKResponse>(r, pNotifyMessage, (a => {
                            Log.Debug($"Notified {r.ConnectionID} about WorkspaceUpdated");
                        }));
                    } catch(Exception ex) {
                        Log.Error($"Failed to send SendWorkspaceUpdatedRequest to {r.ConnectionID}: {ex.Message}");
                    }
                }
            });
            return true;
        }

        public bool Sub(IRouter pClient, WorkspaceUpdatedSubscribe pMessage) {
            if (!_dicClients.ContainsKey(pClient.ConnectionID)) {
                if (!_dicClients.TryAdd(pClient.ConnectionID, pClient)) {
                    return false;
                }
                pClient.Disconnected += Client_Disconnected;
            }
            return true;
        }

        public bool UnSub(IRouter pClient, WorkspaceUpdatedUnSubscribe pMessage) {
            if (_dicClients.ContainsKey(pClient.ConnectionID)) {
                if (!_dicClients.TryRemove(pClient.ConnectionID, out _)) {
                    Log.Error($"Failed to remove {pClient.ConnectionID}, leaking memory");
                } else {
                    pClient.Disconnected -= Client_Disconnected;
                }
            }
            return true;
        }

        void Client_Disconnected(object sender, IRouter e) {
            UnSub(e, new WorkspaceUpdatedUnSubscribe());
        }
    }
}