namespace AutoRender {
    public enum Status {
        /**
         * Statuses that are based on server returns 
         */

        ProjectMissing,
        
        SourceMissing,
        SourceInvalid,

        TargetExists,
        TargetInvalid,

        Busy,
        Paused,
        Queued,
        Processable,
        Finished,

        Error,
        Unknown, // -- should never happen

        /**
         * Statuses based on client actions
         */
        Updating

        //old
        /*UnScheduled,
        Scheduled,

        Running,
        Success,
        Failed,
        Exists
        */
    }
}
