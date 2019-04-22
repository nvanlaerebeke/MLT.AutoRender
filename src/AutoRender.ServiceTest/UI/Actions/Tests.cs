using ConsoleManager;
using System;
using System.Threading.Tasks;
using WebSocketMessaging;

namespace AutoRender.ServiceTest {
    public class Tests {
        public Tests() { }

        public Menu GetMenu() {
            Menu objMenu = new Menu();
            objMenu.Add(new MenuItem("1", "Subscribe While Busy", "Start rendering a project and try to subscribe while buys", delegate () {
                var objSubscribeWhileBusyTest = new SubscribeWhileBusy();
                objSubscribeWhileBusyTest.Start();
            }));
            return objMenu;
        }
    }
}
