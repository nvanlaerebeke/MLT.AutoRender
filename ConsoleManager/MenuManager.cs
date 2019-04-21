using System;
using System.Collections.Generic;

namespace ConsoleManager {
    public class MenuManager {
        public Menu _objMenu;

        public MenuManager(Menu pMenu) {
            _objMenu = pMenu;
            _objMenu.menuChanged += delegate (object sender, EventArgs e) {
                Console.Clear();
                Console.Write(_objMenu.ToString());
            };
        }

        public void Start() {
            Console.Write(_objMenu.ToString());
            while (true) {
                Console.Write("=> ");
                string option = Console.ReadLine().ToLower();
                if (option == "h") {
                    Console.Write(_objMenu.ToString());
                } else {
                    var found = false;
                    List<MenuItem> currentMenu = _objMenu.GetCurrentMenuItems();
                    foreach (MenuItem objMenuItem in currentMenu) {
                        if (objMenuItem.Option.ToLower() == option) {
                            found = true;
                            objMenuItem?.Callback.Invoke();
                        }
                    }
                    if(!found) {
                        Console.WriteLine("Invalid Input");
                    }
                }
            }
        }
    }
}
