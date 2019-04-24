using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using AutoRender.Video;
using System.Reflection;
using AutoRender.Data;
using log4net;

namespace AutoRender.MLT {

    /// <summary>
    /// ToDo: Cleanup
    /// </summary>
    public class MeltConfig {
        private readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, string> _dicConsumerProperties = null;
        private string _strSourceFile;
        private XDocument _objConfig = null;
        private readonly VideoInfoCache VideoInfoCache;

        public string TargetPath { get; private set; }

        public string SourceFile {
            get {
                return _strSourceFile;
            }
            set {
                if (File.Exists(value)) {
                    _strSourceFile = value;

                    string strRes = "";
                    if (!String.IsNullOrEmpty(_strSourceFile)) {
                        var objInfo = VideoInfoCache.Get(_strSourceFile);
                        if (objInfo != null && !string.IsNullOrEmpty(objInfo.Height)) {
                            strRes = " " + objInfo.Height + "p";
                        }
                    }
                    var strName = new FileInfo(Path.GetFileName(Project.FullPath)).Name.Replace(".mlt", "") + strRes + " [TV]";
                    TargetPath = Path.Combine(Settings.FinalDirectory, strName, strName + ".mp4"); //.Replace(" ", "_");
                }
            }
        }

        internal string ConfigFile { get; private set; }
        public string TempSourcePath { get; private set; }
        public string TempTargetPath { get; private set; }
        private MLTProject Project { get; set; }

        internal MeltConfig(MLTProject pProject, VideoInfoCache pVideoInfoCache) {
            VideoInfoCache = pVideoInfoCache;
            Project = pProject;
            ConfigFile = Path.Combine(Settings.TempDirectory, Project.ID.ToString(), Path.ChangeExtension(Path.GetFileName(Project.FullPath), ".xml"));
            try {
                LoadConfig();
                DetectSource();
                TempTargetPath = Path.Combine(Settings.TempDirectory, Project.ID.ToString(), Path.ChangeExtension(Path.GetFileName(TargetPath), ".tmp"));
            } catch (Exception ex) {
                //log and ignore?
                Log.Error(ex);
            }
        }

        internal void SetTargetName(string pFileName) {
            TargetPath = CrazyUtils.PathHelper.NormalizeAbsolutePath(Path.Combine(Settings.FinalDirectory, pFileName));
        }

        internal void Reload() {
            LoadConfig();
        }

        internal void WriteConfig() {
            if (File.Exists(SourceFile)) {
                FixSourceFile();
                AddConsumer();
            } else {
                Log.Info("Source file missing, not adding consumer to config yet");
                return;
            }

            bool locked = false;
            do {
                try {
                    var strConfig = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + _objConfig.ToString();
                    File.WriteAllText(ConfigFile, strConfig);
                } catch (IOException) {
                    //locked
                    locked = true;
                    System.Threading.Thread.Sleep(100);
                }
            } while (locked);
        }

        private void LoadConfig() {
            do {
                try {
                    _objConfig = XDocument.Load(Project.FullPath);
                } catch (IOException) {
                    //locked
                    System.Threading.Thread.Sleep(100);
                } catch (InvalidOperationException) {
                } catch (System.Xml.XmlException) {
                    Log.Error("Invalid XML");
                    return;
                }
            } while (_objConfig == null);
        }

        private void AddConsumer() {
            //working
            //  <consumer  target="../Final/testing.mp4" preset="ultrafast" f="mp4" vcodec="libx264" real_time="-1" threads="0" height="720" width="1280" crf="40" deinterlace_method="yadif" rescale="bilinear" top_field_first="2" r="25" mbd="rd" progressive="1" subcmp="satd" bf="2"  ab="384k" ac="2" acodec="acc" g="15"  ar="48000" trellis="1" mlt_service="avformat" b_strategy="1" channels="2" cmp="satd" />

            VideoInfo objInfo = VideoInfoCache.Get(SourceFile);
            if (string.IsNullOrEmpty(SourceFile)) {
                return;
            }

            _dicConsumerProperties = Settings.ConsumerProperties; //get settings from configuration
            _dicConsumerProperties["target"] = TempTargetPath;//Path.Combine(Settings.FinalDirectory, TargetPath);

            //VIDEO
            _dicConsumerProperties["vcodec"] = objInfo.VideoCodec;
            _dicConsumerProperties["width"] = objInfo.Width;
            _dicConsumerProperties["height"] = objInfo.Height;
            //_dicConsumerProperties["bf"] = objInfo.VideoSettings["has_b_frames"];

            //_dicConsumerProperties["frame_rate_num"] = "30000";
            //_dicConsumerProperties["frame_rate_den"] = "1001";
            /*var strFrameRate = objInfo.VideoSettings["r_frame_rate"];
            if (strFrameRate.Contains("/")) {
                var arrParts = strFrameRate.Split('/');
                _dicConsumerProperties["frame_rate_num"] = arrParts[0];
                _dicConsumerProperties["frame_rate_den"] = arrParts[1];
            }*/

            //AUDIO
            _dicConsumerProperties["acodec"] = objInfo.AudioCodec;
            _dicConsumerProperties["ar"] = objInfo.AudioSampleRate; // -- sample rate https://www.mltframework.org/plugins/ConsumerAvformat/#ar
            _dicConsumerProperties["ab"] = objInfo.AudioBitRate; // -- bitrate https://www.mltframework.org/plugins/ConsumerAvformat/#ab
            //_dicConsumerProperties["channels"] = objInfo.AudioSettings["channels"];

            //Some testing params
            //Use lossess compression, = same as source
            //https://trac.ffmpeg.org/wiki/Encode/H.264#LosslessH.264
            //_dicConsumerProperties["crf"] = _dicConsumerProperties["crf"];

            //impacts quality per filesize
            //https://trac.ffmpeg.org/wiki/Encode/H.264#a2.Chooseapresetandtune
            //_dicConsumerProperties["preset"] = "ultrafast";
            //_dicConsumerProperties["crf"] = "45";

            //GOP = keyframe interval, shotcut used 150, recommended  = 250
            //_dicConsumerProperties["g"] = _dicConsumerProperties["g"]; // = "250"

            //create consumer element
            XElement objEl = new XElement("consumer");
            foreach (var objKvp in _dicConsumerProperties) {
                objEl.Add(new XAttribute(objKvp.Key, objKvp.Value));
            }

            //add consumer below the profile
            _objConfig.Element("mlt").Descendants("profile").First().AddAfterSelf(objEl);

            //we need a root element set in the mlt file
            _objConfig.Root.SetAttributeValue("root", Settings.ProjectDirectory);
        }

        private void DetectSource() {
            var strSourcePath = "";
            if (String.IsNullOrEmpty(strSourcePath)) {
                var strProjectNamePath = CrazyUtils.PathHelper.NormalizeAbsolutePath(Path.Combine(Settings.NewDirectory, Path.GetFileName(Project.FullPath)));
                if (File.Exists(strProjectNamePath)) {
                    strSourcePath = strProjectNamePath;
                }
            }

            var sections = _objConfig.Descendants().ToList();
            var resources = sections.Where(s => s.Name == "property" && s.Attribute("name").Value == "resource").ToList();
            foreach (var resource in resources) {
                var strFullPath = CrazyUtils.PathHelper.NormalizeAbsolutePath(Path.Combine(Settings.NewDirectory, Path.GetFileName(resource.Value))); ;
                if (File.Exists(strFullPath)) {
                    if (String.IsNullOrEmpty(strSourcePath)) {
                        strSourcePath = strFullPath;
                    }
                }
            }
            if (!String.IsNullOrEmpty(strSourcePath)) {
                SourceFile = strSourcePath;
            }
        }

        private void FixSourceFile() {
            //apply the new found path
            TempSourcePath = Path.Combine(Settings.TempDirectory, Project.ID.ToString(), Path.GetFileName(SourceFile));
            _objConfig.Descendants().Where(s => s.Name == "property" && s.Attribute("name").Value == "resource").ToList().ForEach(r => {
                if (!r.Value.EndsWith("black", StringComparison.CurrentCulture)) {
                    r.Value = TempSourcePath;
                }
            });
        }
    }
}