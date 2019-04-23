namespace AutoRender.Data {

    public class WorkspaceItemUpdate {
        public WorkspaceItem WorkspaceItem;
        public WorkspaceAction Action;

        public WorkspaceItemUpdate(WorkspaceItem pWorkpaceItem, WorkspaceAction pAction) {
            WorkspaceItem = pWorkpaceItem;
            Action = pAction;
        }
    }
}