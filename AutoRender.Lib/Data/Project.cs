using System;
using System.IO;
using System.Linq;
using AutoRender.Lib.Melt;
using CrazyUtils;

namespace AutoRender.Lib {
    public class Project : Base {
        public event EventHandler ProjectChanged;

        private FileInfo _objProjectFile;
        private VideoInfo _objTargetInfo;
        private VideoInfo _objSourceInfo;

        private MeltJob Job { get; set; }

        #region Properties
        public Guid ID { get; private set; }
        internal MeltConfig Config { get; private set; }
        public string SourcePath { 
            get { 
                return Config.SourceFile; 
            } 
            set {
                if (Config.SourceFile != value) {
                    Config.SourceFile = value;
                    Reload();
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
        public string TargetName { get { return Path.GetFileName(Config.TargetPath); } set { Config.SetTargetName(value); Reload(); } }
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
                    var objJob = MeltJobScheduler.GetAll().Where(c => c.Project.Name.Equals(this.Name)).FirstOrDefault();
                    if (objJob == null || objJob.Status != JobStatus.Running) {
                        //calculate it again
                        if (_objTargetInfo == null) {
                            _objTargetInfo = VideoInfo.Get(TargetPath);
                        }
                        return _objTargetInfo.IsValid;
                    } else {
                        _objTargetInfo = null;// -- it's invalid, we're busy with it
                        return false; // -- we're busy with it, just return false
                    }
                }
                return false;
            }
        }
        #endregion

        public Project(string pFullPath) {
            ID = Guid.NewGuid();
            Directory.CreateDirectory(Path.Combine(Settings.TempDirectory, ID.ToString()));

            _objProjectFile = new FileInfo(pFullPath);
            Config = new MeltConfig(this);

            Job = new MeltJob(this); // -- ToDo: pass null or the config for the job, not the project itself
            Job.ProgressChanged += (object sender, EventArgs e) => { ProjectChanged?.Invoke(sender, e); };
            Job.StatusChanged += (object sender, EventArgs e) => {
                Reload();
                ProjectChanged?.Invoke(sender, e);
            };

            if (TargetExists) { _objTargetInfo = VideoInfo.Get(TargetPath); }
            if (SourceExists) { _objSourceInfo = VideoInfo.Get(SourcePath); }
        }

        #region Methods
        internal void Reload() {
            Config.Reload();
            if (TargetExists) { _objTargetInfo = VideoInfo.Get(TargetPath); }
            if (SourceExists) { _objSourceInfo = VideoInfo.Get(SourcePath); }
        }

        public void Start() {
            if(Status == ProjectStatus.Paused) {
                Job.Resume();
            } else if (!TargetExists && SourceExists && Status != ProjectStatus.Busy) {
                Config.WriteConfig();
                Job.Schedule();
            }
        }

        public void Stop() { Job.Stop(); }
        public void Pause() { Job.Pause(); }
        #endregion

        #region Equals
        public static bool Equals(Project obj1, Project obj2) {
            if (obj1 == obj2) { return true; } // compare reference/value(null)
            if (obj1 != null) { //check if obj1 isn't null, obj2 in this case is set so they're not equal
                return (obj1.Equals(obj2));
            }
            return false;
        }

        public bool Equals(Project pProject) {
            return (
                pProject != null &&
                this.SourceExists.Equals(pProject.SourceExists) &&
                this.SourceIsValid.Equals(pProject.SourceIsValid) &&
                this.TargetExists.Equals(pProject.TargetExists) &&
                this.TargetIsValid.Equals(pProject.TargetIsValid) &&
                this.TargetName.Equals(pProject.TargetName) &&
                this.TargetPath.Equals(pProject.TargetPath) &&
                this.FullPath.Equals(pProject.FullPath) &&
                this.Job.Equals(pProject.Job) &&
                this.Name.Equals(pProject.Name) &&
                this.SourcePath.Equals(pProject.SourcePath));
        }

        public override int GetHashCode() {
            return this.GetHashCodeFromFields(FullPath, Name, TargetName, TargetExists, TargetPath);
        }
        public override bool Equals(object obj) {
            return this.Equals(obj as Project);
        }
        #endregion
    }
}
