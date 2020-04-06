using System;
using System.IO;
using CrazyUtils.Config;

namespace AutoRender.Client.Config {

    public class Settings : RegistryConfig {

        static Settings() {
            ApplicationName = "AutoRender";
            RegistryROOT = "HKCU";
        }

        public static string ProjectPath {
            get {
                return Get(nameof(ProjectPath), Path.Combine(@"C:\Inbox", "Projects"));
            }
            set {
                if (!ProjectPath.Equals(value)) {
                    Set(nameof(ProjectPath), value);
                }
            }
        }

        public static string NewDirectoryPath {
            get {
                return Get(nameof(NewDirectoryPath), Path.Combine(@"C:\Inbox", "Onbewerkt"));
            }
            set {
                if (!NewDirectoryPath.Equals(value)) {
                    Set(nameof(NewDirectoryPath), value);
                }
            }
        }

        public static string ShotcutExecutable {
            get {
                return Get(nameof(ShotcutExecutable), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Shotcut", "Shotcut.exe"));
            }
            set {
                if (!ShotcutExecutable.Equals(value)) {
                    Set(nameof(ShotcutExecutable), value);
                }
            }
        }

        public static string HostName {
            get {
                return Get(nameof(HostName), "localhost");
            }
            set {
                if (!HostName.Equals(value)) {
                    Set(nameof(HostName), value);
                }
            }
        }

        public static int Port {
            get {
                return Get(nameof(Port), 37697);
            }
            set {
                if (!HostName.Equals(value)) {
                    Set(nameof(Port), value);
                }
            }
        }
    }
}