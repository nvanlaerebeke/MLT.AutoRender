using System;
using System.Collections.Generic;

namespace ConsoleManager {
    public class Menu {
        public List<string> tree = new List<string>();

        public event EventHandler menuChanged;

        private List<MenuItem> _lstMenu;
        public List<MenuItem> MenuItems {
            get {
                return _lstMenu;
            }
        }

        public Menu() {
            _lstMenu = new List<MenuItem>();
            //add help menu item
            _lstMenu.Add(new MenuItem("h", "help", "print the help menu", delegate () {
                Console.WriteLine(this.ToString());
            }));
        }

        public void Add(MenuItem pMenuItem) {
            foreach (MenuItem objMenuItem in _lstMenu) {
                if (objMenuItem.Option == pMenuItem.Option) {
                    return;
                    throw new Exception(String.Format("menu option {0} already found", pMenuItem.Option));
                }
            }
            AddSubmenuEvents(pMenuItem);
            _lstMenu.Add(pMenuItem);
        }

        private void AddSubmenuEvents(MenuItem pMenuItem) {
            if (pMenuItem.SubMenu != null && pMenuItem.SubMenu.MenuItems.Count > 0) {
                pMenuItem.submenuEntered += delegate (object sender, EventArgs e) {
                    tree.Add((sender as MenuItem).Option);
                    menuChanged?.Invoke(this, new EventArgs());
                };

                pMenuItem.submenuExited += delegate (object sender, EventArgs e) {
                    tree.RemoveAt(tree.Count - 1);
                    menuChanged?.Invoke(this, new EventArgs());
                };

                foreach (MenuItem objMenuItem in pMenuItem.SubMenu.MenuItems) {
                    if (objMenuItem.SubMenu != null && objMenuItem.SubMenu.MenuItems.Count > 0) {
                        AddSubmenuEvents(objMenuItem);

                    }
                }
            }
        }

        public List<MenuItem> GetCurrentMenuItems() {
            List<MenuItem> subMenu = _lstMenu;
            for (int i = 0; i < tree.Count; i++) {
                foreach (MenuItem objMenuItem in subMenu) {
                    if (objMenuItem.Option == tree[i]) {
                        subMenu = objMenuItem.SubMenu.MenuItems;
                        break;
                    }
                }
            }
            return subMenu;
        }

        public override string ToString() {
            string strOutput = "List of available commands" + Environment.NewLine;
            foreach (MenuItem objMenuItem in GetCurrentMenuItems()) {
                strOutput += "\t-" + objMenuItem.Option + "\t" + objMenuItem.Title + ((!String.IsNullOrEmpty(objMenuItem.Description)) ? ": " + objMenuItem.Description : "") + Environment.NewLine;
            }
            strOutput += Environment.NewLine;
            return strOutput;
        }

    }
}
