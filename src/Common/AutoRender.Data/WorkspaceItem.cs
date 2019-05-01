using System;

namespace AutoRender.Data {

    public class WorkspaceItem {
        public Guid ID { get; set; }
        public Project Project { get; set; }
        public VideoInfo New { get; set; }
        public VideoInfo Final { get; set; }
    }
}