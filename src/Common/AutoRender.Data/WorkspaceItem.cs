using System;

namespace AutoRender.Data {

    public class WorkspaceItem {

        public WorkspaceItem() {
        }

        public WorkspaceItem(Guid pID) {
            ID = pID;
        }

        public Guid ID { get; set; }
        public Project Project { get; set; }
        public VideoInfo New { get; set; }
        public VideoInfo Final { get; set; }
    }
}