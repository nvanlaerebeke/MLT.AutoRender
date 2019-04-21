using System.Windows;
using System.Windows.Controls;

namespace AutoRender.UserControls {
    public partial class EditableTextField : UserControl {
        private string _strOriginalText = "";

        public EditableTextField() {
            InitializeComponent();
        }

        #region Properties


        public string DisplayText {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public bool IsEditing {
            get { return (bool)GetValue(IsEditingProperty); }
            set {
                if(value) {
                    _strOriginalText = DisplayText;
                }
                SetValue(IsEditingProperty, value);
            }
        }

        public bool AllowEditing {
            get { return (bool)GetValue(AllowEditingProperty); }
            set { SetValue(AllowEditingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayText.  This enables animation, styling, binding, etc...
        public static DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(EditableTextField), new PropertyMetadata(""));
        public static DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableTextField), new PropertyMetadata(false));
        public static DependencyProperty AllowEditingProperty = DependencyProperty.Register("AllowEditing", typeof(bool), typeof(EditableTextField), new PropertyMetadata(true));

        #endregion

        #region Actions
        public event RoutedEventHandler TextChanged;
        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            if (AllowEditing) { IsEditing = true; }
        }
        private void btnStopEditing_Click(object sender, RoutedEventArgs e) {
            IsEditing = false;
            if (!_strOriginalText.Equals(DisplayText)) {
                TextChanged?.Invoke(sender, e);
            }
        }
        #endregion
    }
}
