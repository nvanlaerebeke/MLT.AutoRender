using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutoRender.Messaging {
    public class WorkspaceItemUpdate {
        public WorkspaceItem WorkspaceItem;
        public WorkspaceAction Action;

        public WorkspaceItemUpdate(Lib.WorkspaceItem pWorkpaceItem, Lib.WorkspaceAction pAction) {
            WorkspaceItem = new WorkspaceItem(pWorkpaceItem);
            Action = (WorkspaceAction)Enum.Parse(typeof(WorkspaceAction), pAction.ToString());
        }

        [JsonConstructor]
        public WorkspaceItemUpdate(WorkspaceItem pWorkspaceItem, WorkspaceAction pAction) {
            WorkspaceItem = pWorkspaceItem;
            Action = pAction;
        }
        public enum WorkspaceAction {
            New,
            Deleted,
            Updated
        }
    }
}
