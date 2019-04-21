using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebSocketMessaging;

namespace AutoRender.Messaging {
    public class MessageProvider : WebSocketMessaging.MessageProvider {
        private static List<Type> _lstCustomTypes = new List<Type>();

        protected override List<Type> GetAllTypes() {
            lock (_lstCustomTypes) {
                if (_lstCustomTypes.Count == 0) {
                    _lstCustomTypes.Clear();

                    foreach (string strNamespace in new string[] {
                            this.GetType().Namespace + ".Event",
							this.GetType().Namespace + ".Notification",
							this.GetType().Namespace + ".Request",
                            this.GetType().Namespace + ".Response",
                            this.GetType().Namespace + ".Subscribe",
                            this.GetType().Namespace + ".UnSubscribe",
                            this.GetType().Namespace + ".Action.Event",
                            this.GetType().Namespace + ".Action.Notification",
                            this.GetType().Namespace + ".Action.Request",
                            this.GetType().Namespace + ".Action.Subscribe",
                            this.GetType().Namespace + ".Action.UnSubscribe",

                    }) {
                        var lstAll = (from t in Assembly.GetExecutingAssembly().GetTypes()
                                      where t.IsClass && t.Namespace == strNamespace
                                      select t).ToList();

                        foreach (Type objType in lstAll) {
                            if (
                                objType.IsSubclassOf(typeof(RequestMessage)) ||
                                objType.IsSubclassOf(typeof(ResponseMessage)) ||
                                objType.IsSubclassOf(typeof(NotificationMessage)) ||
                                objType.IsSubclassOf(typeof(EventMessage)) ||
                                objType.IsSubclassOf(typeof(SubscribeMessage)) ||
                                objType.IsSubclassOf(typeof(UnSubscribeMessage)) ||
                                objType.IsSubclassOf(typeof(SubscriptionHandler)) ||
                                objType.Namespace.Contains(this.GetType().Namespace + ".Action.") //objType.IsSubclassOf(typeof(WebSocketMessaging.Action.BaseAction)) <= generic type, this is easier
                            ) {
                                _lstCustomTypes.Add(objType);
                            }
                        }
                    }
                }
                return _lstCustomTypes;
            }
        }
    }
}
