using System;
using System.Collections.Generic;
using System.IO;

namespace AutoRender.Server.Config {

    public class Settings : CrazyUtils.Config.RegistryConfig {

        public static event EventHandler WorkspaceSourceUpdated;

        static Settings() {
            ApplicationName = "AutoRender";
            RegistryROOT = "HKLM";
        }

        private static string AppPath {
            get {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Shotcut");
            }
        }

        public static string MeltPath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "melt");
                } else {
                    return Get("MeltPath", Path.Combine(AppPath, "qmelt.exe"));
                }
            }
            set {
                if (!MeltPath.Equals(value)) {
                    Set("MeltPath", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static string BasePath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Get("BasePath", Path.Combine("/mnt/nas/Video/Movies", "Inbox"));
                } else {
                    return Get("BasePath", Path.Combine(@"C:\", "Inbox"));
                }
            }
            set {
                if (!BasePath.Equals(value)) {
                    Set("BasePath", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static string ProjectDirectory {
            get {
                return Get("ProjectDirectory", Path.Combine(BasePath, "Projects"));
            }
            set {
                if (!ProjectDirectory.Equals(value)) {
                    Set("ProjectDirectory", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static string FinalDirectory {
            get {
                return Get("FinalDirectory", Path.Combine(BasePath, "Final"));
            }
            set {
                if (!FinalDirectory.Equals(value)) {
                    Set("FinalDirectory", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static string NewDirectory {
            get {
                return Get("NewDirectory", Path.Combine(BasePath, "Onbewerkt"));
            }
            set {
                if (!NewDirectory.Equals(value)) {
                    Set("NewDirectory", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static string TempDirectory {
            get {
                return Path.Combine(Path.GetTempPath(), "AutoRender");
            }
        }

        public static string LogDirectory {
            get {
                return Get("LogDirectory", Path.Combine(BasePath, "Log"));
            }
            set {
                if (!LogDirectory.Equals(value)) {
                    Set("LogDirectory", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static int Threads {
            get {
                return Get("Threads", 2);
            }
            set {
                if (!Threads.Equals(value)) {
                    Set("Threads", value);
                    WorkspaceSourceUpdated?.Invoke(null, null);
                }
            }
        }

        public static Dictionary<string, string> ConsumerProperties {
            get {
                return new Dictionary<string, string> {
                    //crf sets quality
                    //https://trac.ffmpeg.org/wiki/Encode/H.264#a1.ChooseaCRFvalue
                    { "crf",  "23" },
                    //{ "preset",  "slower" },
                    { "preset",  "fast" },
                    //{ "tune", "film" },

                    //required settings melt
                    { "mlt_service",  "avformat" },
                    { "target",  "" },

                    //format we export in
                    {"f",  "mp4" },

                    //resizing params - hardcoded, no need to change this
                    { "rescale",  "bilinear" },
                    //{ "progressive",  "1" },

                    //Video codec settings
                    { "vcodec",  "libx264" }, // -- x264 by default
                    { "movflags",  "+faststart" }, // -- puts all info in the beginning of the file
                    { "threads",  "0" }, // -- 0 = optimal

                    //mlt threads, not clear what this needs to be,crashes when < -1
                    //lower than 0 means number of none-frame droppping threads, but 1 seems kinda low
                    //https://www.mltframework.org/plugins/ConsumerAvformat/#real_time
                    //https://www.mltframework.org/faq/#does-mlt-take-advantage-of-multiple-cores-or-how-do-i-enable-parallel-processing
                    { "real_time",  "-1" },

                    // when files are interlaced, deinterlace them
                    {"deinterlace_method",  "yadif" },
                    {"top_field_first",  "2" },
                    //{"trellis",  "2" },

                    //settings overwritten by source file
                    {"bf",  "3" }, // -- 16 recommended by ffmpeg https://sites.google.com/site/linuxencoding/x264-ffmpeg-mapping
                    {"b_strategy",  "2" },
                    {"height",  "720" },
                    {"width",  "1280" },

                    //Final frame rate: r = num/den (24000/1001=23,97...)
                    //{"frame_rate_den",  "1000" }, // devider
                    //{"frame_rate_num",  "25000" }, //frame rate num
                    //{"r",  "30" },

                    //GOP = keyframe interval, recommended in 250
                    { "g",  "150" }, // -- https://sites.google.com/site/linuxencoding/x264-ffmpeg-mapping

                    //audio
                    { "acodec",  "aac" }, // default audio codec
                    //{ "ar",  "48000"  }, // -- sample rate: https://www.mltframework.org/plugins/ConsumerAvformat/#ar
                    //{ "ab",  "384k" }, // -- bitrate
                    //{ "ac",  "2" },
                    //{ "channels",  "2" },

                    //{ "mbd",  "rd" },
                    //{ "subcmp",  "satd" },
                    //{ "cmp",  "satd" },
                    //{ "aspect",  "1,77778" },
                };
            }
        }
    }
}