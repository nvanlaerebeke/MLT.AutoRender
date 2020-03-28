using System.Collections.Generic;
using System.Linq;
using AutoRender.Server.Services;

namespace AutoRender.Server {

    internal class ServiceManager {
        private List<IService> Services = new List<IService>();

        public ServiceManager() {
            Services.Add(new CommunicationService());
            Services.Add(new LoggingService());
            Services.Add(new WorkspaceMonitorService());
        }

        public void Start() {
            Services = Services.OrderByDescending(s => s.Priority).ToList();
            Services.ForEach(s => s.Start());
        }

        public void Stop() {
            Services.ForEach(s => s.Stop());
        }
    }
}