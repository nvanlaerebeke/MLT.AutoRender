using System;
namespace AutoRender.Messaging {
    public enum ProjectStatus {
        SourceMissing,
        SourceInvalid,

        TargetExists,
        TargetInvalid,

        Busy,
        Paused,
        Queued,
        Processable,
        Finished,

        Error
    }
}
