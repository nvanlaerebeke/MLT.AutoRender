namespace AutoRender.Workspace {

    public static class WorkspaceFactory {
        private static Workspace Workspace;

        public static Workspace Get() {
            if(Workspace == null) {
                Workspace = new Workspace();
                Workspace.ReLoad();
            }
            return Workspace;
        }
    }
}