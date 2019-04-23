using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using AutoRender.Data;
using AutoRender.Messaging;

namespace AutoRender {

    public partial class WorkspaceItemViewModel : BaseViewModel {

        //fields
        public WorkspaceItem WorkspaceItem { get; private set; }

        private bool _blnUpdating = false; // -- are we updating the row? - server/client communication
        private bool _blnSelectedForHandling; // -- row selected for rendering when pressing start?
        private Timer _objBusyTimer;

        public WorkspaceItemViewModel(WorkspaceItem pItem) {
            WorkspaceItem = pItem;
        }

        public Guid ID { get { return WorkspaceItem.ID; } }

        public Status Status {
            get {
                if (IsUpdating) { return Status.Updating; }

                if (WorkspaceItem.Project != null) { //  -- there is a project, check the source and target files if everything is ok
                    return (Status)Enum.Parse(typeof(Status), WorkspaceItem.Project.Status.ToString());
                }
                if (WorkspaceItem.New != null) { // -- there is no project but there is a new file
                    return Status.ProjectMissing;
                }
                if (WorkspaceItem.Final != null) { //there is a final file but no project or source file, so something that was finished
                    return Status.Finished;
                }
                return Status.Unknown;
            }
        }

        public string Info {
            get {
                switch (Status) {
                    case Status.Finished:
                        if (WorkspaceItem.Project != null && WorkspaceItem.Project.TimeTaken != 0) {
                            TimeSpan objSpan = new TimeSpan(0, 0, (int)WorkspaceItem.Project.TimeTaken);
                            return String.Format("Finished in {0} hours, {1} minutes and {2} seconds", objSpan.Hours, objSpan.Minutes, objSpan.Seconds);
                        }
                        return "";

                    case Status.ProjectMissing:
                        return "Missing project, new file?";

                    case Status.SourceMissing:
                        return "Source file is missing";

                    case Status.TargetExists:
                        return "Output file already exists";

                    case Status.TargetInvalid:
                        return "Failed exporting file";

                    case Status.SourceInvalid:
                        return "Invalid source file";

                    case Status.Busy:
                        if (_objBusyTimer == null) {
                            _objBusyTimer = new Timer(1000);
                            _objBusyTimer.Elapsed += _objBusyTimer_Elapsed;
                            _objBusyTimer.Start();
                        }
                        TimeSpan objElapsed = new TimeSpan(0, 0, (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - WorkspaceItem.Project.StartTime));
                        return String.Format("Elapsed time: {0} hours, {1} minutes and {2} seconds", objElapsed.Hours, objElapsed.Minutes, objElapsed.Seconds);

                    case Status.Processable:
                    case Status.Queued:
                    case Status.Updating:
                    case Status.Paused:
                        return "";

                    case Status.Error:
                        return "An error has occurred";

                    default:
                        return "Unknown status";
                }
            }
        }

        private void _objBusyTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (Status != Status.Busy) {
                _objBusyTimer.Elapsed -= _objBusyTimer_Elapsed;
                _objBusyTimer.Stop();
                _objBusyTimer.Close();
                _objBusyTimer = null;
            }
            OnPropertyChanged("Info");
        }

        public bool IsUpdating {
            get { return _blnUpdating; }
            set {
                _blnUpdating = value;
                OnPropertyChanged();
                OnPropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Is the current WorkspaceItemViewModel row enabled
        /// When updating this should say false
        /// </summary>
        public bool IsEnabled {
            get {
                return !(Status == Status.Updating);
            }
        }

        internal void Update(WorkspaceItem pWorkspaceItem) {
            WorkspaceItem = pWorkspaceItem;
            OnPropertyChanged("");
        }
    }
}