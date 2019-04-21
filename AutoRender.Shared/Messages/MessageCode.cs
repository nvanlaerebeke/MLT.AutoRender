namespace AutoRender.Messaging {
    public enum MessageCode {
        GetStatus = 0x01,
        UpdateProjectTarget = 0x02,
        WorkspaceUpdated = 0x03,
        JobStart = 0x04,
        JobStop = 0x05,
        JobPause = 0x06,
        UpdateProjectSource = 0x07,
        Reload = 0x08
    }
}
