namespace AutoRender.Data {

    public class WorkspaceUpdatedEventArgs : System.EventArgs {
        public WorkspaceItem WorkspaceItem { get; set; }
        public WorkspaceAction Action { get; set; }

        public WorkspaceUpdatedEventArgs(WorkspaceItem pItem, WorkspaceAction pAction) {
            WorkspaceItem = pItem;
            Action = pAction;
        }
    }
}