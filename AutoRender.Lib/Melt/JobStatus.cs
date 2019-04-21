namespace AutoRender.Lib.Melt {
    public enum JobStatus {
        UnScheduled,
        Scheduled,

        Running,
        Success,
        Failed,
        Exists,
        Paused
    }
}
