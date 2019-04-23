using System.Threading.Tasks;
using System.Windows;

namespace AutoRender {
    internal delegate void WindowStatusChanged(WindowStatus pStatus, string pMessage);

    internal enum WindowStatus {
        Busy,
        Ready
    }
    public class BaseWindow : Window {
        protected TaskFactory uiFactory;
        internal event WindowStatusChanged StatusChanged;
        public BaseWindow() : base() {
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected void SetLoading(string pMessage) {
            StatusChanged?.Invoke(WindowStatus.Busy, pMessage);
            ContentEnabled(false);
        }

        protected void EndLoading() {
            StatusChanged?.Invoke(WindowStatus.Ready, "");
            ContentEnabled(true);
        }

        private void ContentEnabled(bool pEnabled) {
            uiFactory.StartNew(() => {
                if (Content is UIElement objRoot) {
                    if (pEnabled) {
                        objRoot.IsEnabled = true;
                        objRoot.Focusable = true;
                        objRoot.IsHitTestVisible = true;
                    } else {
                        objRoot.IsEnabled = false;
                        objRoot.Focusable = false;
                        objRoot.IsHitTestVisible = false;
                    }
                }
            });
        }
    }
}