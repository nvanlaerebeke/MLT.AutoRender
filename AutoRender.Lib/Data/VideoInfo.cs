using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRender.Lib {

    public class VideoInfo : CrazyUtils.Base {
        private static List<VideoInfoCache> _lstVideoInfoCache = new List<VideoInfoCache>();
        private Process _objProcess;
        private Thread _thdStdOut;
        private Thread _thdStdErr;

        private ManualResetEvent _objReadBlocker = new ManualResetEvent(false);
        private Dictionary<string, string> _dicVideoSettings = new Dictionary<string, string>();
        private Dictionary<string, string> _dicAudioSettings = new Dictionary<string, string>();

        public Boolean IsValid { get; private set; }
        public Dictionary<string, string> VideoSettings { get { return _dicVideoSettings; } }
        public Dictionary<string, string> AudioSettings { get { return _dicAudioSettings; } }
        public string Path { get; private set; }
        public string FileName { get { return System.IO.Path.GetFileName(Path); } }
        public TimeSpan Duration { get; set; }


        private class VideoInfoCache {
            internal FileInfo FileInfo { get; set; }
            internal VideoInfo VideoInfo { get; set; }
            public VideoInfoCache(FileInfo pFileInfo, VideoInfo pVideoInfo) {
                FileInfo = pFileInfo;
                VideoInfo = pVideoInfo;
            }
        }

        /// <summary>
        /// Returns the VideoInfo from a cache
        /// </summary>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static VideoInfo Get(string pPath) {
            FileInfo objFileInfo = new FileInfo(pPath);
            //Get the item that has the same path as the one we're trying to get
            var objItem = _lstVideoInfoCache.FirstOrDefault(c => c.FileInfo.FullName.Equals(objFileInfo.FullName));
            if (objItem != null) { // -- is null if not yet cached
                if (CrazyUtils.PathHelper.FileEquals(objFileInfo, objItem.FileInfo)) {
                    return objItem.VideoInfo; // -- file not changed, return cached version
                }
                //File updated, update cache
                objItem.FileInfo = objFileInfo;
                objItem.VideoInfo = new VideoInfo(objFileInfo.FullName);

                return objItem.VideoInfo;
            }
            //Not yet cached, cache and return it
            var objCache = new VideoInfoCache(objFileInfo, new VideoInfo(objFileInfo.FullName));
            _lstVideoInfoCache.Add(objCache);
            return objCache.VideoInfo;
        }



        private VideoInfo(string pPath) {
            Path = pPath;
            IsValid = new Helpers.VideoHelper(Path).Valid();
            if (IsValid) {
                Update();
            }
        }

        private void Update() {
            Task.Run(() => {
                _objProcess = new Process {
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
                _objProcess.Exited += _objProcess_Exited;

                Log.Debug(String.Format("Running {0} with params: {1}", Settings.FfprobePath, "-show_streams -i \"" + Path + "\""));


                _thdStdErr = new Thread(readStdError) {
                    IsBackground = true
                };

                _thdStdOut = new Thread(readStdOut) {
                    IsBackground = true
                };

                try {
                    _objProcess.Start();
                    _thdStdOut.Start();
                    _thdStdErr.Start();
                } catch (Exception ex) {
                    Log.Error(ex);
                }
            });
            _objReadBlocker.WaitOne();
            ParseSettings();
        }

        private void readStdOut() {
            StreamReader srStdOut = null;

            try {
                srStdOut = _objProcess.StandardOutput;
                string strLine = String.Empty;

                strLine = srStdOut.ReadLine();
                if (!String.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (_objProcess != null)) {
                    if (strLine.Trim().Length != 0) {
                        HandleLine(strLine);
                    }
                    strLine = _objProcess.StandardOutput.ReadLine();
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

        private void readStdError() {
            StreamReader srStdErr = null;
            try {
                srStdErr = _objProcess.StandardError;
                string strLine = String.Empty;

                strLine = srStdErr.ReadLine();
                if (!String.IsNullOrEmpty(strLine)) {
                    Console.WriteLine(strLine);
                }
                while ((strLine != null) && (_objProcess != null)) {
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

        private void _objProcess_Exited(object sender, System.EventArgs e) {
            try {
                _objProcess.Exited -= _objProcess_Exited;
                _objProcess = null;

                foreach (KeyValuePair<int, Dictionary<string, string>> dicSettings in _dicSettings) {
                    if (dicSettings.Value.ContainsKey("codec_name")) {
                        switch (dicSettings.Value["codec_name"]) {
                            case "h264":
                                _dicVideoSettings = dicSettings.Value;
                                break;
                            case "aac":
                                _dicAudioSettings = dicSettings.Value;
                                break;
                        }
                    }
                }
                _dicSettings = null;
                _currIndex = -1;
            } catch (Exception ex) {
                Log.Error(ex);
            }

            _objReadBlocker.Set();
        }

        private void ParseSettings() {
            if (VideoSettings.ContainsKey("duration")) {
                string[] arrParts = VideoSettings["duration"].Split(',', '.');
                Duration = new TimeSpan(0, 0, 0, int.Parse(arrParts[0]), int.Parse(arrParts[1].Substring(0,3)));
            }
        }

        private int _currIndex = -1;
        private Dictionary<int, Dictionary<string, string>> _dicSettings = new Dictionary<int, Dictionary<string, string>>();
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

        public static bool Equals(VideoInfo obj1, VideoInfo obj2) {
            if (obj1 == obj2) { return true; } // compare reference/value(null)
            if (obj1 != null) { //check if obj1 isn't null, obj2 in this case is set so they're not equal
                return (obj1.Equals(obj2));
            }
            return false;
        }
        public bool Equals(VideoInfo pVideoInfo) {
            if (pVideoInfo == null) { return false; }

            return (
                 this.IsValid.Equals(pVideoInfo.IsValid) &&
                 this.Path.Equals(pVideoInfo.Path) &&
                 (
                    this.AudioSettings == pVideoInfo.AudioSettings || // -- same obj(reference)
                    (
                        this.AudioSettings.Count == pVideoInfo.AudioSettings.Count && // -- if count is different...
                        this.AudioSettings.Except(pVideoInfo.AudioSettings).Any() // -- get the differences
                    )
                 ) &&
                 (
                    this.VideoSettings == pVideoInfo.VideoSettings ||
                    (
                        this.VideoSettings.Count == pVideoInfo.VideoSettings.Count &&
                        this.VideoSettings.Except(pVideoInfo.VideoSettings).Any()
                    )
                 )
             );
        }

        public override int GetHashCode() {
            return this.GetHashCodeFromFields(this.AudioSettings, this.IsValid, this.Path, this.VideoSettings);
        }
        public override bool Equals(object obj) {
            return this.Equals(obj as VideoInfo);
        }
    }
}