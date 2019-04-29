using AutoRender.Data;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.Video {

    /// <summary>
    /// ToDo: Split the reading/getting of all settings and creating/returning of the VideoInfo
    /// </summary>
    public class VideoInfoReader {
        private readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string Path;

        private Process Process;
        private readonly ManualResetEvent ReadBlocker = new ManualResetEvent(false);

        private Dictionary<int, Dictionary<string, string>> _dicSettings = new Dictionary<int, Dictionary<string, string>>();

        public Dictionary<string, string> VideoSettings { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AudioSettings { get; private set; } = new Dictionary<string, string>();

        private int _currIndex = -1;
        public TimeSpan Duration { get; private set; }

        public VideoInfoReader(string pPath) {
            Path = pPath;
        }

        public VideoInfo Read() {
            if (!new VideoValidator(Path).IsValid()) {
                Log.Error($"Invalid Video File: {Path}");
                return null;
            }

            RunParser();

            return new VideoInfo() {
                Name = new FileInfo(Path).Name,
                Path = Path,

                Duration = Duration,
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
        }

        private void RunParser() {
            Task.Run(() => {
                Process = new Process {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo(Settings.FfprobePath, "-show_streams -i \"" + Path + "\"") {
                        UseShellExecute = false,
                        ErrorDialog = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = Settings.TempDirectory
                    },
                };
                Process.Exited += Process_Exited;

                Log.Debug(String.Format("Running {0} with parameters: {1}", Settings.FfprobePath, "-show_streams -i \"" + Path + "\""));

                try {
                    Process.Start();
                    new Thread(ReadStdError) { IsBackground = true }.Start();
                    new Thread(ReadStdOut) { IsBackground = true }.Start();
                } catch (Exception ex) {
                    Log.Error(ex);
                }
            });
            ReadBlocker.WaitOne();
            ParseSettings();
        }

        private void ReadStdOut() {
            StreamReader srStdOut = null;

            try {
                srStdOut = Process.StandardOutput;
                string strLine = string.Empty;

                strLine = srStdOut.ReadLine();
                if (!string.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (Process != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                    }
                    strLine = Process.StandardOutput.ReadLine();
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                if (srStdOut != null) {
                    srStdOut.Close();
                    srStdOut.Dispose();
                }
            }
        }

        private void ReadStdError() {
            StreamReader srStdErr = null;
            try {
                srStdErr = Process.StandardError;
                string strLine = string.Empty;

                strLine = srStdErr.ReadLine();
                if (!string.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (Process != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                    }
                    strLine = srStdErr.ReadLine();
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                if (srStdErr != null) {
                    srStdErr.Close();
                    srStdErr.Dispose();
                }
            }
        }

        private void Process_Exited(object sender, EventArgs e) {
            try {
                Process.Exited -= Process_Exited;
                Process = null;

                foreach (KeyValuePair<int, Dictionary<string, string>> dicSettings in _dicSettings) {
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
                _dicSettings = null;
                _currIndex = -1;
            } catch (Exception ex) {
                Log.Error(ex);
            }

            ReadBlocker.Set();
        }

        private void ParseSettings() {
            if (VideoSettings.ContainsKey("duration")) {
                string[] arrParts = VideoSettings["duration"].Split(',', '.');
                Duration = new TimeSpan(0, 0, 0, int.Parse(arrParts[0]), int.Parse(arrParts[1].Substring(0, 3)));
            }
        }

        private void HandleLine(string pLine) {
            if (!String.IsNullOrEmpty(pLine)) {
                string[] arrParts = pLine.Split('=');
                if (arrParts.Length == 2) {
                    if (arrParts[0] == "index") {
                        _currIndex = int.Parse(arrParts[1]);
                    }
                    if (_currIndex != -1) {
                        if (!_dicSettings.ContainsKey(_currIndex)) {
                            _dicSettings.Add(_currIndex, new Dictionary<string, string>());
                        }
                        _dicSettings[_currIndex][arrParts[0]] = arrParts[1];
                    }
                }
            }
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