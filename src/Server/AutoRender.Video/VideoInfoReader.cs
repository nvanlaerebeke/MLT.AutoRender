using AutoRender.Data;
using CrazyUtils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.Video {

    /// <summary>
    /// ToDo: Split the reading/getting of all settings and creating/returning of the VideoInfo
    /// </summary>
    public class VideoInfoReader {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string Path;

        private ProcessRunner Process;
        private readonly ManualResetEvent ReadBlocker = new ManualResetEvent(false);

        private ConcurrentDictionary<int, ConcurrentDictionary<string, string>> _dicSettings = new ConcurrentDictionary<int, ConcurrentDictionary<string, string>>();

        public ConcurrentDictionary<string, string> VideoSettings { get; private set; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, string> AudioSettings { get; private set; } = new ConcurrentDictionary<string, string>();

        private int _currIndex = -1;

        public VideoInfoReader(string pPath) {
            Path = pPath;
            Process = new ProcessRunner(Settings.FfprobePath, "-show_streams -i \"" + Path + "\"");
            Log.Info($"Running: {Settings.FfmpegPath} -show_streams -i \"{Path}\"");
        }

        public Task<VideoInfo> Read() {
            return Task.Run(() => { 
                if (!new VideoValidator(Path).IsValid().Result) {
                    Log.Error($"Invalid Video File: {Path}");
                    return null;
                }

                Process.StatusChanged += Process_StatusChanged;
                Process.StdOut += Process_StdOut;

                Process.Start();
                ReadBlocker.WaitOne();

                Process.StatusChanged -= Process_StatusChanged;
                Process.StdOut -= Process_StdOut;

                return new VideoInfo() {
                    Name = new FileInfo(Path).Name,
                    Path = Path,

                    Duration = GetDuration(),
                    IsValid = true,

                    //Video
                    Height = (VideoSettings.ContainsKey("height")) ? VideoSettings["height"] : "",
                    Width = (VideoSettings.ContainsKey("width")) ? VideoSettings["width"] : "",

                    VideoCodec = GetVideoCodec(),

                    //Audio
                    AudioBitRate = GetAudioBitrate(),
                    AudioSampleRate = (AudioSettings.ContainsKey("sample_rate")) ? AudioSettings["sample_rate"] : "",
                    AudioCodec = GetAudioCodec()
                };
            });
        }

        void Process_StatusChanged(object sender, ProcessStatus e) {
            switch(e) {
                case ProcessStatus.Done:
                    ProcessSettings();
                    ReadBlocker.Set();
                    break;
                case ProcessStatus.Failed:
                case ProcessStatus.Stopped:
                    ReadBlocker.Set();
                    break;
                case ProcessStatus.Paused:
                case ProcessStatus.Running:
                    break;
            }
        }

        void Process_StdOut(object sender, string e) {
            if (!String.IsNullOrEmpty(e)) {
                Log.Info(e);
                string[] arrParts = e.Split('=');
                if (arrParts.Length == 2) {
                    if (arrParts[0] == "index") {
                        _currIndex = int.Parse(arrParts[1]);
                    }
                    if (_currIndex != -1) {
                        if (!_dicSettings.ContainsKey(_currIndex)) {
                            _dicSettings.TryAdd(_currIndex, new ConcurrentDictionary<string, string>());
                        }
                        _dicSettings[_currIndex][arrParts[0]] = arrParts[1];
                    }
                }
            }
        }

        private void ProcessSettings() {
            foreach (KeyValuePair<int, ConcurrentDictionary<string, string>> dicSettings in _dicSettings) {
                if (dicSettings.Value.ContainsKey("codec_name")) {
                    switch (dicSettings.Value["codec_name"]) {
                        case "h264":
                            VideoSettings = dicSettings.Value;
                            break;

                        case "aac":
                            AudioSettings = dicSettings.Value;
                            break;
                    }
                }
            }
        }

        private TimeSpan GetDuration() {
            if (VideoSettings.ContainsKey("duration")) {
                var arrParts = VideoSettings["duration"].Split(',', '.');
                return new TimeSpan(0, 0, 0, int.Parse(arrParts[0]), int.Parse(arrParts[1].Substring(0, 3)));
            }
            return new TimeSpan();
        }

        private string GetVideoCodec() {
            if (VideoSettings.ContainsKey("codec_name")) {
                switch (VideoSettings["codec_name"]) {
                    case "h264":
                        return "libx264";
                }
            }
            return "libx264";
        }

        private string GetAudioCodec() {
            if (AudioSettings.ContainsKey("codec_name")) {
                switch (AudioSettings["codec_name"]) {
                    case "aac":
                        return "aac";
                }
            }
            return "aac";
        }

        private string GetAudioBitrate() {
            var intBitrate = (AudioSettings.ContainsKey("bit_rate")) ? Math.Round((double)(int.Parse(AudioSettings["bit_rate"]) / 1000)) : 384;
            if (intBitrate <= 16) {
            } else if (intBitrate <= 32) {
                return "32k";
            } else if (intBitrate <= 48) {
                return "48k";
            } else if (intBitrate <= 96) {
                return "96k";
            } else if (intBitrate <= 128) {
                return "128k";
                //} else if (intBitrate <= 220) {
            } else if (intBitrate <= 256) {
                return "256k";
            } else if (intBitrate <= 384) {
                return "384k";
                //} else if (intBitrate <= 512) {
            }
            return "384k";
        }
    }
}