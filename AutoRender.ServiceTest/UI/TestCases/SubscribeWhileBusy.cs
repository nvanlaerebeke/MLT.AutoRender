using ConsoleManager;
using System;
using System.Threading.Tasks;
using WebSocketMessaging;

namespace AutoRender.ServiceTest {
    public class SubscribeWhileBusy {
        private Connection _objConnection;
        public SubscribeWhileBusy() {
            _objConnection = new Connection();
            _objConnection.Connected += _objConnection_Connected;
        }

        private void _objConnection_Connected(object sender, EventArgs e) {
            Console.WriteLine("Getting status...");
            var objWorkspaceItems = _objConnection.Request<AutoRender.Messaging.Response.GetStatus>(new AutoRender.Messaging.Request.GetStatus());
            
            if (objWorkspaceItems.Status == ResponseCode.Success) {
                Console.WriteLine("Status gotten, found " + objWorkspaceItems.WorkspaceItems.Count + " workspaceitems");
                if (objWorkspaceItems.WorkspaceItems.Count > 0) {
                    Console.WriteLine("Starting first job...");
                    var objStart = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Request.JobStart(objWorkspaceItems.WorkspaceItems[0].ID));
                    if (objStart.Status == ResponseCode.Success) {
                        Console.WriteLine("Job Started, waiting 10 seconds for job to start rendering...");
                        System.Threading.Thread.Sleep(10000); // -- wait a little so that the server can start rendering

                        WebSocketMessaging.Response.ACK objSubscribe;
                        do {
                            Console.WriteLine("Subscribing on updates ...");
                            objSubscribe = _objConnection.Request<WebSocketMessaging.Response.ACK>(new AutoRender.Messaging.Subscribe.WorkspaceUpdated());
                            System.Threading.Thread.Sleep(2000);
                        } while (objSubscribe == null || objSubscribe.Status != ResponseCode.Success);
                        Console.WriteLine("Subscribe done");
                    }
                }
            }
        }

        public void Start() {
            _objConnection.Connect();
        }
    }
}
