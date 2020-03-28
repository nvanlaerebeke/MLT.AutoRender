using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace AutoRender.Workspace.Monitor {

    internal delegate void FSEvent(List<FSEventInfo> pEvents);

    public class Monitor {

        internal event FSEvent Changed;

        private readonly FileSystemWatcher _objWatcher;
        private readonly List<FSEventInfo> _lstEvents = new List<FSEventInfo>();

        private readonly Timer _objWaitTimer;

        public Monitor(FileSystemWatcher pWatcher) {
            _objWatcher = pWatcher;
            _objWaitTimer = new Timer(2000);
            _objWaitTimer.Elapsed += (sender, e) => {
                _objWaitTimer.Stop();
                lock (_lstEvents) {
                    if (_lstEvents.Count > 0) {
                        var lstCopy = new List<FSEventInfo>(_lstEvents);
                        _lstEvents.Clear();
                        Changed?.Invoke(new List<FSEventInfo>(lstCopy));
                    }
                }
            };
            _objWaitTimer.AutoReset = false;
        }

        public void Start() {
            try {
                _objWatcher.Changed += _objWatcher_Changed;
                _objWatcher.Created += _objWatcher_Created;
                _objWatcher.Deleted += _objWatcher_Deleted;
                _objWatcher.Renamed += _objWatcher_Renamed;
                _objWatcher.EnableRaisingEvents = true;
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        public void Stop() {
            try {
                _objWatcher.EnableRaisingEvents = false;
                _objWatcher.Changed -= _objWatcher_Changed;
                _objWatcher.Created -= _objWatcher_Created;
                _objWatcher.Deleted -= _objWatcher_Deleted;
                _objWatcher.Renamed -= _objWatcher_Renamed;
                _objWatcher.Dispose();
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void _objWatcher_Renamed(object sender, RenamedEventArgs e) {
            Add(sender, e);
            ResetTimer();
        }

        private void _objWatcher_Deleted(object sender, FileSystemEventArgs e) {
            Add(sender, e);
            ResetTimer();
        }

        private void _objWatcher_Created(object sender, FileSystemEventArgs e) {
            Add(sender, e);
            ResetTimer();
        }

        private void _objWatcher_Changed(object sender, FileSystemEventArgs e) {
            Add(sender, e);
            ResetTimer();
        }

        private void ResetTimer() {
            if (_objWaitTimer != null) {
                try {
                    _objWaitTimer.Stop();
                    _objWaitTimer.Start();
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }
        }

        private void Add(object sender, FileSystemEventArgs eventArgs) {
            lock (_lstEvents) {
                try {
                    if (!_lstEvents.Any(e => e.Args.FullPath == eventArgs.FullPath && eventArgs.ChangeType == e.Args.ChangeType)) {
                        _lstEvents.Add(new FSEventInfo(sender, eventArgs));
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}