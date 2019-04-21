using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoRender.UserControls {
    public partial class ProjectField : UserControl {
        public ProjectField() {
            InitializeComponent();
        }

        #region Properties

        public string ProjectName {
            get { return (string)GetValue(ProjectNameProperty); }
            set {
                SetValue(ProjectNameProperty, value);
                ShowCreate = String.IsNullOrEmpty(value);
            }
        }

        public bool ShowCreate {
            get { return (bool)GetValue(ShowCreateProperty); }
            set { SetValue(ShowCreateProperty, value); }
        }

        // Using a DependencyProperty as the store for ProjectName.  This enables animation, styling, binding, etc...
        public static DependencyProperty ProjectNameProperty = DependencyProperty.Register("ProjectName", typeof(string), typeof(ProjectField), new PropertyMetadata(""));
        public static DependencyProperty ShowCreateProperty = DependencyProperty.Register("ShowCreate", typeof(bool), typeof(ProjectField), new PropertyMetadata(true));
        #endregion


        #region Actions
        public event RoutedEventHandler CreateProjectClicked;

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            if(ShowCreate) {
                CreateProjectClicked?.Invoke(sender, e);
            }
        }
        #endregion
    }
}