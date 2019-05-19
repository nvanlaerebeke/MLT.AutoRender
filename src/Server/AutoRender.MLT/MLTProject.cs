using AutoRender.Data;
using AutoRender.Video;
using log4net;
using System;
using System.IO;
using System.Reflection;

namespace AutoRender.MLT {

    /// <summary>
    /// ToDo: Cleanup and only use this in MLT
    ///       Anywhere else use the Project object
    ///       Also this class does too much - should only describe the project
    /// </summary>
    public class MLTProject {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<MLTProject> ProjectChanged;

        private FileInfo _objProjectFile;
        private VideoInfo _objTargetInfo;
        private VideoInfo _objSourceInfo;
        public VideoInfoCache VideoInfoCache;

        public MeltJob Job { get; private set; }

        #region Properties

        public Guid ID { get; private set; }
        public MeltConfig Config { get; private set; }

        public string SourcePath {
            get {
                return Config.SourceFile;
            }
            set {
                if (Config.SourceFile != value) {
                    Config.SourceFile = value;
                    Reload();
                    ProjectChanged?.Invoke(this, this);
                }
            }
        }

        public bool TargetExists {
            get {
                if (File.Exists(TargetPath)) {
                    return true;
                }
                return false;
            }
        }

        public string TargetPath { get { return Config.TargetPath; } }
        public bool SourceIsValid { get { return (SourceExists && _objSourceInfo != null && _objSourceInfo.IsValid); } }
        public int Progress { get { return Job.Percentage; } }
        public string FullPath { get { return _objProjectFile.FullName; } }
        public string Name { get { return _objProjectFile.Name; } }
        public bool SourceExists { get { return (File.Exists(Config.SourceFile)); } }

        //ToDo make changing the name(consumer) in the config possible
        public string TargetName { 
            get { 
                return Path.GetFileName(Config.TargetPath); 
            } 
            set { 
                Config.SetTargetName(value); 
                Reload();
                ProjectChanged?.Invoke(this, this);
            } 
        }

        public double TimeTaken { get { return Job.TimeTaken; } }
        public long StartTime { get { return Job.StartTime; } }

        public ProjectStatus Status {
            get {
                if (Job.Status == JobStatus.Running) { return ProjectStatus.Busy; }
                if (Job.Status == JobStatus.Scheduled) { return ProjectStatus.Queued; }
                if (Job.Status == JobStatus.Paused) { return ProjectStatus.Paused; }
                if (TargetExists) {
                    return (TargetIsValid) ? ProjectStatus.Finished : ProjectStatus.Error;
                }

                if (!SourceExists) { return ProjectStatus.SourceMissing; }
                if (SourceExists && !SourceIsValid) { return ProjectStatus.SourceInvalid; }
                return ProjectStatus.Processable;
            }
        }

        public bool TargetIsValid {
            get {
                if (TargetExists) {
                    if (Job.Status != JobStatus.Running) {
                        return (_objTargetInfo != null) && _objTargetInfo.IsValid;
                    }
                    return false; // -- we're busy with it, just return false
                }
                return false;
            }
        }

        #endregion Properties

        public MLTProject(string pFullPath, VideoInfoCache pVideoInfoCache) {
            VideoInfoCache = pVideoInfoCache;
            ID = Guid.NewGuid();
            Directory.CreateDirectory(Path.Combine(Settings.TempDirectory, ID.ToString()));

            _objProjectFile = new FileInfo(pFullPath);

            Config = new MeltConfig(this, VideoInfoCache);
            if (TargetExists) { _objTargetInfo = VideoInfoCache.Get(TargetPath); }
            if (SourceExists) { _objSourceInfo = VideoInfoCache.Get(SourcePath); }

            Job = new MeltJob(this); // -- ToDo: pass null or the config for the job, not the project itself
            Job.ProgressChanged += (object sender, System.EventArgs e) => { ProjectChanged?.Invoke(sender, this); };
            Job.StatusChanged += (object sender, JobStatus e) => {
                Log.Info("Project was changed - notify everyone");
                switch (e) {
                    case JobStatus.Failed:
                    case JobStatus.Success:
                        Reload();
                        break;
                    case JobStatus.Paused:
                    case JobStatus.Running:
                    case JobStatus.Scheduled:
                    case JobStatus.UnScheduled:
                        break;
                }
                ProjectChanged?.Invoke(sender, this);
            };
        }

        #region Methods

        public void Reload() {
            Config.Reload();
            if (TargetExists) { _objTargetInfo = VideoInfoCache.Get(TargetPath); }
            if (SourceExists) { _objSourceInfo = VideoInfoCache.Get(SourcePath); }
        }

        public void Schedule() {
            //MeltJobScheduler.Schedule(Job);
            if (Status == ProjectStatus.Paused) {
                MeltJobScheduler.GetScheduler().Schedule(Job);
            } else if (!TargetExists && SourceExists && Status != ProjectStatus.Busy) {
                MeltJobScheduler.GetScheduler().Schedule(Job);
            }
        }

        public void Stop() {
            Job.Stop();
        }

        public void Pause() {
            Job.Pause();
        }

        #endregion Methods

        #region Equals

        public static bool Equals(Project obj1, Project obj2) {
            if (obj1 == obj2) { return true; } // compare reference/value(null)
            if (obj1 != null) { //check if obj1 isn't null, obj2 in this case is set so they're not equal
                return (obj1.Equals(obj2));
            }
            return false;
        }

        public bool Equals(MLTProject pProject) {
            return (
                pProject != null &&
                SourceExists.Equals(pProject.SourceExists) &&
                SourceIsValid.Equals(pProject.SourceIsValid) &&
                TargetExists.Equals(pProject.TargetExists) &&
                TargetIsValid.Equals(pProject.TargetIsValid) &&
                TargetName.Equals(pProject.TargetName) &&
                TargetPath.Equals(pProject.TargetPath) &&
                FullPath.Equals(pProject.FullPath) &&
                Job.Equals(pProject.Job) &&
                Name.Equals(pProject.Name) &&
                SourcePath.Equals(pProject.SourcePath));
        }

        public override bool Equals(object obj) {
            return Equals(obj as Project);
        }

        #endregion Equals

        public Project GetProject() {
            return new Project() {
                Progress = Progress,
                ProjectName = Name,
                SourceExists = SourceExists,
                SourceIsValid = SourceIsValid,
                SourceName = Path.GetFileName(SourcePath),
                StartTime = StartTime,
                Status = Status.ToString(),
                TargetExists = TargetExists,
                TargetIsValid = TargetIsValid,
                TargetName = TargetName,
                TimeTaken = TimeTaken
            };
        }
    }
}