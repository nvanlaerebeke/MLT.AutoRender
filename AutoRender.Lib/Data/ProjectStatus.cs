using System;
namespace AutoRender.Lib {
    public enum ProjectStatus {
        SourceMissing,
        SourceInvalid,

        TargetExists,
        TargetInvalid,

        Busy,
        Queued,
        Paused,
        Processable,
        Finished,

        Error
    }
}
