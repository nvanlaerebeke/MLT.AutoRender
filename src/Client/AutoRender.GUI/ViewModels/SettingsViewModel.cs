namespace AutoRender {

    public class SettingsViewModel : BaseViewModel {

        public SettingsViewModel() {
        }

        #region Client

        private string _strLocalProjectDirectory = "";
        private string _strShotcutExecutable = "";
        private string _strLocalNewDirectory = "";

        public string ShotcutExecutable {
            get {
                return _strShotcutExecutable;
            }
            set {
                if (!_strShotcutExecutable.Equals(value)) {
                    _strShotcutExecutable = value;
                    OnPropertyChanged(nameof(ShotcutExecutable));
                }
            }
        }

        public string LocalProjectDirectory {
            get {
                return _strLocalProjectDirectory;
            }
            set {
                if (!_strLocalProjectDirectory.Equals(value)) {
                    _strLocalProjectDirectory = value;
                    OnPropertyChanged(nameof(LocalProjectDirectory));
                }
            }
        }

        public string LocalNewDirectory {
            get {
                return _strLocalNewDirectory;
            }
            set {
                if (!_strLocalNewDirectory.Equals(value)) {
                    _strLocalNewDirectory = value;
                    OnPropertyChanged(nameof(LocalNewDirectory));
                }
            }
        }

        #endregion Client

        #region Server

        private string _strServerFinalDirectory = "";
        private string _strServerLogDirectory = "";
        private string _strServerMeltPath = "";
        private string _strServerNewDirectory = "";
        private string _strServerProjectDirectory = "";
        private string _strStorageLocation = "";
        private string _strBackupLocation = "";

        private int _intThreads = 2;

        private string _strHostName = "";
        private int _intPort = 80;

        public string HostName {
            get {
                return _strHostName;
            }
            set {
                if (!_strHostName.Equals(value)) {
                    _strHostName = value;
                    OnPropertyChanged(nameof(HostName));
                }
            }
        }

        public int Port {
            get {
                return _intPort;
            }
            set {
                if (!_intPort.Equals(value)) {
                    _intPort = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        public string ServerFinalDirectory {
            get {
                return _strServerFinalDirectory;
            }
            set {
                if (!_strServerFinalDirectory.Equals(value)) {
                    _strServerFinalDirectory = value;
                    OnPropertyChanged(nameof(ServerFinalDirectory));
                }
            }
        }

        public string ServerLogDirectory {
            get {
                return _strServerLogDirectory;
            }
            set {
                if (!_strServerLogDirectory.Equals(value)) {
                    _strServerLogDirectory = value;
                    OnPropertyChanged(nameof(ServerLogDirectory));
                }
            }
        }

        public string ServerMeltPath {
            get {
                return _strServerMeltPath;
            }
            set {
                if (!_strServerMeltPath.Equals(value)) {
                    _strServerMeltPath = value;
                    OnPropertyChanged(nameof(ServerMeltPath));
                }
            }
        }

        public string ServerNewDirectory {
            get {
                return _strServerNewDirectory;
            }
            set {
                if (!_strServerNewDirectory.Equals(value)) {
                    _strServerNewDirectory = value;
                    OnPropertyChanged(nameof(ServerNewDirectory));
                }
            }
        }

        public string ServerProjectDirectory {
            get {
                return _strServerProjectDirectory;
            }
            set {
                if (!_strServerProjectDirectory.Equals(value)) {
                    _strServerProjectDirectory = value;
                    OnPropertyChanged(nameof(ServerProjectDirectory));
                }
            }
        }

        public int Threads {
            get {
                return _intThreads;
            }
            set {
                if (!_intThreads.Equals(value)) {
                    _intThreads = value;
                    OnPropertyChanged(nameof(Threads));
                }
            }
        }

        public string StorageLocation {
            get {
                return _strStorageLocation;
            }
            set {
                if (!_strStorageLocation.Equals(value)) {
                    _strStorageLocation = value;
                    OnPropertyChanged(nameof(StorageLocation));
                }
            }
        }

        public string BackupLocation {
            get {
                return _strBackupLocation;
            }
            set {
                if (!_strBackupLocation.Equals(value)) {
                    _strBackupLocation = value;
                    OnPropertyChanged(nameof(BackupLocation));
                }
            }
        }

        #endregion Server
    }
}