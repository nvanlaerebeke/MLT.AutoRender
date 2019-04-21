using ConsoleManager;
using System;
using System.Threading.Tasks;
using WebSocketMessaging;

namespace AutoRender.ServiceTest {
    public class General {
        private Connection _objConnection;
        public General() {
            _objConnection = new Connection();
        }
        public void Ping() {
            Console.WriteLine("Sending ping...");
            
            var pong = _objConnection.Request<WebSocketMessaging.Response.Pong>(new WebSocketMessaging.Request.Ping());
            if (pong.Status != ResponseCode.Success) {
                Console.WriteLine("Failed sending ping");
            } else {
                Console.WriteLine("Response received");
            }
        }

        public void GetStatus() {
            Console.WriteLine("Sending GetStatus...");

            var response = _objConnection.Request<AutoRender.Messaging.Response.GetStatus>(new AutoRender.Messaging.Request.GetStatus());
            if (response.Status != ResponseCode.Success) {
                Console.WriteLine("Failed getting status");
            } else {
                Console.WriteLine("Response received");
            }
        }

        public Menu GetMenu() {
            Menu objMenu = new Menu();
            objMenu.Add(new MenuItem("c", "Connect", "Connect to the server", delegate () {
                _objConnection.Connect();
            }));
            objMenu.Add(new MenuItem("d", "Disconnect", "Disconnect from the server", delegate () {
                _objConnection.Disconnect();
            }));
            objMenu.Add(new MenuItem("p", "ping", "Sends a ping to the server", delegate () {
                Ping();
            }));
            objMenu.Add(new MenuItem("s", "GetStatus", "Gets the Workspace status", delegate () {
                GetStatus();
            }));
            objMenu.Add(new MenuItem("n", "notify", "Sends a notification message to the server", delegate () {
                _objConnection.Notify(new WebSocketMessaging.Notification.Info("Notify"));
            }));
            return objMenu;
        }
    }
}
