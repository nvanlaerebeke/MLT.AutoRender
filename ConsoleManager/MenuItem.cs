using System;

namespace ConsoleManager {
    public class MenuItem {
        public string Option { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Menu SubMenu { get; private set; }

        public Action Callback;
        public event EventHandler submenuEntered;
        public event EventHandler submenuExited;

        public MenuItem(string pShortOption, string pTitle, string pDescription, Action pCallback) {
            Option = pShortOption;
            Title = pTitle;
            Description = pDescription;
            SubMenu = null;
            Callback = delegate () { pCallback?.Invoke(); };
        }

        public MenuItem(string pShortOption, string pTitle, string pDescription, Menu pSubmenu, Action pCallback) {
            Option = pShortOption;
            Title = pTitle;
            Description = pDescription;
            SubMenu = (pSubmenu != null && pSubmenu.MenuItems.Count > 0) ? pSubmenu : null;

            SubMenu.Add(new MenuItem("q", "quit", "Quit this menu", delegate () {
                submenuExited?.Invoke(this, new EventArgs());
            }));

            Callback = delegate () {
                submenuEntered?.Invoke(this, new EventArgs());
                pCallback?.Invoke();
            };
        }
    }
}
