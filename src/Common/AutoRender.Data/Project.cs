using AutoRender.Data;

namespace AutoRender.Data {

    public class Project {
        public int Progress { get; set; }
        public string ProjectName { get; set; }
        public bool SourceExists { get; set; }
        public bool SourceIsValid { get; set; }
        public string SourceName { get; set; }
        public long StartTime { get; set; }
        public string Status { get; set; }
        public bool TargetExists { get; set; }
        public bool TargetIsValid { get; set; }
        public string TargetName { get; set; }
        public double TimeTaken { get; set; }
    }
}