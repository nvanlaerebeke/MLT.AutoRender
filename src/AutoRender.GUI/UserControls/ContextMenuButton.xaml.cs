using System.Collections.Generic;
using System.Windows.Controls;

namespace AutoRender.UserControls {
    /// <summary>
    /// Interaction logic for ContextMenuButton.xaml
    /// </summary>
    public partial class ContextMenuButton : UserControl {
        public ContextMenuButton() {
            InitializeComponent();
        }

        private void btnContextMenuOpen_Click(object sender, System.Windows.RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn != null && this.ContextMenu != null) {
                this.ContextMenu.DataContext = this.DataContext;
                this.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                this.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
    }
}