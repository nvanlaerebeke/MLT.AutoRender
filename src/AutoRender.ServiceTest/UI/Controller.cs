using ConsoleManager;
using System;
using System.Collections.Generic;

namespace AutoRender.ServiceTest {
    public static class Controller {

        private static Operations Operations { get; set; }

        public static void Start( ) {
            Operations = new Operations();

            MenuManager objManager = new MenuManager(GetMainMenu());
            objManager.Start();
        }
        private static Menu GetMainMenu() {
            Menu objMenu = new Menu();

            //Main Menu Actions
            objMenu.Add(new MenuItem("g", "General actions", "navigates into the general actions menu", Operations.General.GetMenu(), delegate () { }));
            objMenu.Add(new MenuItem("t", "Test cases", "navigates into the test cases menu", Operations.Tests.GetMenu(), delegate () { }));
            return objMenu;
        }
    }
}
