using CrazyUtils;
using System;
using System.IO;

namespace AutoRender {

    public enum Section {
        Global,
    }

    public static class Settings {

        private static string BasePath {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "BasePath", @"\\nas.crazyzone.be\Video\Movies\Inbox\"); }
        }

        private static string ShotcutDirectoryPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Shotcut");
            }
        }

        public static string ProjectPath {
            get { return Path.Combine(BasePath, ConfigManager.Get<string>(Section.Global.ToString(), "ProjectDir", "Projects")); }
        }

        public static string NewDirectoryName {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "NewDir", "Onbewerkt"); }
        }

        public static string ShotcutExecutable {
            get { return Path.Combine(ShotcutDirectoryPath, "shotcut.exe"); }
        }

        public static string Server {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "Server", "localhost"); }
        }

        public static string LocationName {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "LocationName", Environment.MachineName); }
        }

        public static string LocationID {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "LocationID", Guid.NewGuid().ToString()); }
        }
    }
}