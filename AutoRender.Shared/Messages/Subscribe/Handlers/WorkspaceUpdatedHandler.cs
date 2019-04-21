using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketMessaging;

namespace AutoRender.Messaging.Subscribe {
    public class WorkspaceUpdatedHandler : SubscriptionHandler {
        private List<Client> _lstSubscriptions = new List<Client>();

        public override void Notify(RequestMessage pMessage) {
            lock (_lstSubscriptions) {
                foreach (Client objClient in _lstSubscriptions) {
                    try {
                        objClient.Send<WebSocketMessaging.Response.ACK>(pMessage);
                    } catch (Exception ex) {
                        Log.Error(ex);
                    }
                }
            }
        }

        public override void Subscribe(Client pClient) {
            lock (_lstSubscriptions) {
                //clean up clients with the same id, id = unique
                var lstToRemove = _lstSubscriptions.Where(c => c.ClientInfo.ID.Equals(pClient.ClientInfo.ID)).ToList();
                lstToRemove.ForEach(c => {
                    c.Close();
                    _lstSubscriptions.Remove(c);
                });
                pClient.disconnected += pClient_Disconnected;
                _lstSubscriptions.Add(pClient);
            }
        }

        public override void UnSubscribe(Client pClient) {
            lock (_lstSubscriptions) {
                var lstToRemove = _lstSubscriptions.Where(c => c.ClientInfo.ID.Equals(pClient.ClientInfo.ID)).ToList();
                lstToRemove.ForEach(c => {
                    c.disconnected -= pClient_Disconnected;
                    _lstSubscriptions.Remove(c);
                });
            }
        }

        private void pClient_Disconnected(Client pClient) {
            UnSubscribe(pClient);
        }
    }
}
