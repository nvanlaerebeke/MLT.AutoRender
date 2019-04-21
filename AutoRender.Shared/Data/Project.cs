using System;
using Newtonsoft.Json;

namespace AutoRender.Messaging {
    public class Project {
        public string ProjectName { get; set; }
        public string TargetName { get; set; }
        public string SourceName { get; set; }

        public bool TargetExists { get; set; }
        public bool SourceExists { get; set; }

        public bool TargetIsValid { get; set; }
        public bool SourceIsValid { get; set; }

        public ProjectStatus Status { get; set; }
        public int Progress { get; set; }
        public double TimeTaken { get; set; }
        public long StartTime { get; set; }

        [JsonConstructor]
        public Project(Lib.Project pProject) {
            if (pProject != null) {
                ProjectName = pProject.Name;
                TargetName = pProject.TargetName;
                SourceName = System.IO.Path.GetFileName(pProject.SourcePath);

                TargetExists = pProject.TargetExists;
                SourceExists = pProject.SourceExists;

                TargetIsValid = pProject.TargetIsValid;
                SourceIsValid = pProject.SourceIsValid;

                Status = (ProjectStatus)Enum.Parse(typeof(ProjectStatus), pProject.Status.ToString());
                Progress = pProject.Progress;

                TimeTaken = pProject.TimeTaken;
                StartTime = pProject.StartTime;
            }
        }
    }
}
