using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRender.UserControls {
    /// <summary>
    /// Interaction logic for EditableField.xaml
    /// </summary>
    public partial class EditableListField : UserControl {
        private string _strOriginalText = "";

        public EditableListField() {
            InitializeComponent();
        }

        #region Properties
        public string SelectedItem {
            get { return (string)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public bool IsEditing {
            get { return (bool)GetValue(IsEditingProperty); }
            set {
                if (value) {
                    _strOriginalText = SelectedItem;
                }
                SetValue(IsEditingProperty, value);
            }
        }

        public bool AllowEditing {
            get { return (bool)GetValue(AllowEditingProperty); }
            set { SetValue(AllowEditingProperty, value); }
        }

        public ObservableCollection<string> ItemsSource {
            get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayText.  This enables animation, styling, binding, etc...
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(string), typeof(EditableListField), new PropertyMetadata(""));
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<string>), typeof(EditableListField));
        public static DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableListField), new PropertyMetadata(false));
        public static DependencyProperty AllowEditingProperty = DependencyProperty.Register("AllowEditing", typeof(bool), typeof(EditableListField), new PropertyMetadata(true));

        #endregion

        #region Local stuff
        public event RoutedEventHandler SelectionChanged;

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            if (AllowEditing) { IsEditing = true; }
        }
        private void btnStopEditing_Click(object sender, RoutedEventArgs e) {
            IsEditing = false;
            if (_strOriginalText == null || !_strOriginalText.Equals(SelectedItem)) {
                SelectionChanged?.Invoke(sender, e);
            }
        }
        #endregion
    }
}
