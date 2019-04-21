using CrazyUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoRender {
    public enum Section {
        Global,
        ConsumerSettings,
        OverrideConsumerSettings
    }

    public static class Settings {

        private static string AppPath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return "";
                } else {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Shotcut");
                }
            }
        }

        public static string SourcePath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../", "melt/source-me");
                }
                return "";
            }
        }

        public static string MeltPath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../", "melt/bin/melt");
                } else {
                    return ConfigManager.Get<string>(Section.Global.ToString(), "MeltPath", Path.Combine(AppPath, "qmelt.exe"));
                }
            }
        }
        public static string FfprobePath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../", "melt/bin/ffprobe");
                } else {
                    return ConfigManager.Get<string>(Section.Global.ToString(), "FfprobePath", Path.Combine(AppPath), "ffprobe.exe");
                }
            }
        }
        public static string FfmpegPath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../", "melt/bin/ffmpeg");
                } else {
                    return ConfigManager.Get<string>(Section.Global.ToString(), "FfmpegPath", Path.Combine(AppPath, @"ffmpeg.exe"));
                }
            }
        }

        public static string BasePath {
            get {
                if (Environment.OSVersion.Platform == PlatformID.Unix) {
                    return ConfigManager.Get<string>(Section.Global.ToString(), "BasePath", Path.Combine("/mnt/nas/Video/Movies", "Inbox"));
                } else {
                    return ConfigManager.Get<string>(Section.Global.ToString(), "BasePath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Inbox"));
                }
            }
        }

        public static string ProjectDirectory {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "ProjectDirectory", Path.Combine(BasePath, "Projects")); }
        }

        public static string FinalDirectory {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "FinalDirectory", Path.Combine(BasePath, "Final")); }
        }

        public static string NewDirectory {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "NewDirectory", Path.Combine(BasePath, "Onbewerkt")); }
        }

        public static string TempDirectory { get { return Path.Combine(Path.GetTempPath(), "AutoRender"); } }

        public static string LogDirectory {
            get { return ConfigManager.Get<string>(Section.Global.ToString(), "LogDirectory", Path.Combine(BasePath, "Log")); }
        }

        public static int Threads {
            get { return ConfigManager.Get<int>(Section.Global.ToString(), "Threads", 2); }
        }

        public static Dictionary<string, string> ConsumerProperties {
            get {
                return new Dictionary<string, string> {
                    //crf sets quality
                    //https://trac.ffmpeg.org/wiki/Encode/H.264#a1.ChooseaCRFvalue
                    { "crf",  "21" },
                    { "preset",  "faster" },
                    { "tune", "film" },

                    //required settings melt
                    { "mlt_service",  "avformat" },
                    { "target",  "" },
                    
                    //format we export in
                    {"f",  "mp4" },

                    //resizing params - hardcoded, no need to change this
                    { "rescale",  "bilinear" },
                    { "progressive",  "1" },

                    //Video codec settings
                    { "vcodec",  "libx264" }, // -- x264 by default
                    //{"movflags",  "+faststart" }, // -- puts all info in the beginning of the file
                    { "threads",  "0" }, // -- 0 = optimal

                    //mlt threads, not clear what this needs to be,crashes when < -1
                    //lower than 0 means number of none-frame droppping threads, but 1 seems kinda low
                    //https://www.mltframework.org/plugins/ConsumerAvformat/#real_time
                    //https://www.mltframework.org/faq/#does-mlt-take-advantage-of-multiple-cores-or-how-do-i-enable-parallel-processing
                    { "real_time",  "-1" },

                    // when files are interlaced, deinterlace them
                    {"deinterlace_method",  "yadif" },
                    {"top_field_first",  "2" },
                    {"trellis",  "1" },
                    
                    //settings overwritten by source file
                    {"bf",  "2" }, // -- 16 recommended by ffmpeg https://sites.google.com/site/linuxencoding/x264-ffmpeg-mapping
                    {"b_strategy",  "1" },
                    {"height",  "720" },
                    {"width",  "1280" },

                    //Final frame rate: r = num/den (24000/1001=23,97...)
                    //{"frame_rate_den",  "1000" }, // devider
                    //{"frame_rate_num",  "25000" }, //frame rate num
                    {"r",  "25" },

                    //GOP = keyframe interval, recommended in 250
                    { "g",  "250" }, // -- https://sites.google.com/site/linuxencoding/x264-ffmpeg-mapping

                    //audio
                    //{ "acodec",  "aac" }, // default audio codec
                    //{ "ar",  "48000"  }, // -- sample rate: https://www.mltframework.org/plugins/ConsumerAvformat/#ar
                    //{ "ab",  "384k" }, // -- bitrate
                    //{ "ac",  "2" }, 
                    //{ "channels",  "2" },

                    { "mbd",  "rd" },
                    { "subcmp",  "satd" },
                    { "cmp",  "satd" },
                    //{ "aspect",  "1,77778" },
                };
                /*return ConfigManager.Get<Dictionary<string, string>>(Section.ConsumerSettings, new Dictionary<string, string> {
                    //required settings melt
                    { "mlt_service",  "avformat" },
                    {"target",  "" },
                    
                    //format we export in
                    {"f",  "mp4" },

                    //resizing params - hardcoded, no need to change this
                    {"rescale",  "bicubic" },
                    {"progressive",  "1" },

                    //Video codec settings
                    {"vcodec",  "libx264" }, // -- x264 by default
                    {"movflags",  "+faststart" }, // -- puts all info in the beginning of the file
                    {"threads",  "0" }, // -- 0 = optimal

                    //mlt threads, not clear what this needs to be,crashes when < -1
                    //lower than 0 means number of none-frame droppping threads, but 1 seems kinda low
                    //https://www.mltframework.org/plugins/ConsumerAvformat/#real_time
                    //https://www.mltframework.org/faq/#does-mlt-take-advantage-of-multiple-cores-or-how-do-i-enable-parallel-processing
                    { "real_time",  "-1" },

                    // when files are interlaced, deinterlace them
                    { "deinterlace_method",  "yadif" },
                    {"top_field_first",  "2" },
                    
                    //settings overwritten by source file
                    { "bf",  "3" }, // -- use 3 by default
                    {"height",  "720" },
                    {"width",  "1280" },
                    {"frame_rate_den",  "1001" }, //these make up the framerate (r=num/den)
                    {"frame_rate_num",  "30000" },

                    //crf sets quality
                    //https://trac.ffmpeg.org/wiki/Encode/H.264#a1.ChooseaCRFvalue
                    { "crf",  "21" },


                    //Don't think we need this
                    //{"aspect",  "1,77778" },
                    
                    //to test
                    { "preset",  "fast" },

                    //GOP = keyframe interval, recommended in 250
                    { "g",  "150" },

                    //audio
                    {"acodec",  "aac" }, // default audio codec
                    {"ar",  "48000"  }, // -- sample rate: https://www.mltframework.org/plugins/ConsumerAvformat/#ar
                    { "ab",  "384k" }, // -- bitrate
                    { "channels",  "2" }, // -- by default use stereo
                });*/
            }
        }
    }
}

