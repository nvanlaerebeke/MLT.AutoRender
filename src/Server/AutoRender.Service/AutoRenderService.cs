using System.ServiceProcess;
using AutoRender.Server;

namespace AutoRender.Service {

    public partial class AutoRenderService : ServiceBase {
        private AutoRenderServer AutoRender;

        public AutoRenderService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            AutoRender = new AutoRenderServer();
            AutoRender.Start();
        }

        protected override void OnStop() {
            AutoRender.Stop();
        }
    }
}