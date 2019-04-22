namespace AutoRender.Workspace {

    public class WorkspaceUpdatedEventArgs : System.EventArgs {
        public WorkspaceItem WorkspaceItem { get; private set; }
        public WorkspaceAction Action { get; private set; }

        public WorkspaceUpdatedEventArgs(WorkspaceItem pItem, WorkspaceAction pAction) {
            WorkspaceItem = pItem;
            Action = pAction;
        }
    }
}