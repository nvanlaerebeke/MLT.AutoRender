using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using AutoRender.Data;
using AutoRender.Video;
using log4net;

namespace AutoRender.MLT {

    /// <summary>
    /// ToDo: Cleanup
    /// </summary>
    public class MeltConfig {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, string> _dicConsumerProperties = null;
        private string _strSourceFile = "";
        private XDocument _objConfig = null;
        private readonly VideoInfoProvider VideoInfoProvider;
        private string _strTargetPath = "";

        public string TargetPath {
            get {
                if (string.IsNullOrEmpty(_strTargetPath)) {
                    _ = SourceFile;
                }
                return _strTargetPath;
            }
            private set {
                _strTargetPath = value;
            }
        }

        public string SourceFile {
            get {
                return _strSourceFile;
            }
            set {
                if (File.Exists(value)) {
                    _strSourceFile = value;

                    var strRes = "";
                    if (!string.IsNullOrEmpty(_strSourceFile)) {
                        var objInfo = VideoInfoProvider.Get(_strSourceFile);
                        if (objInfo != null && objInfo.Height != 0) {
                            strRes = " " + objInfo.Height + "p";
                        }
                    }
                    var strName = new FileInfo(Path.GetFileName(Project.FullPath)).Name.Replace(".mlt", "") + strRes + " [TV]";
                    TargetPath = Path.Combine(Settings.FinalDirectory, strName, strName + ".mp4"); //.Replace(" ", "_");
                }
            }
        }

        internal string ConfigFile {
            get {
                return Path.Combine(
                    Path.Combine(Settings.TempDirectory, Project.ID.ToString()),
                    Path.ChangeExtension(Path.GetFileName(Project.FullPath), ".xml")
                );
            }
        }

        public string TempSourcePath { get; private set; }

        public string TempTargetPath {
            get {
                return Path.Combine(
                    Path.Combine(Settings.TempDirectory, Project.ID.ToString()),
                    Path.ChangeExtension(Path.GetFileName(TargetPath), ".tmp")
                );
            }
        }

        private MLTProject Project { get; set; }

        internal MeltConfig(MLTProject pProject, VideoInfoProvider pVideoInfoProvider) {
            VideoInfoProvider = pVideoInfoProvider;
            Project = pProject;
            try {
                var ProjectTempDir = Path.Combine(Settings.TempDirectory, Project.ID.ToString());
                if (!Directory.Exists(ProjectTempDir)) {
                    _ = Directory.CreateDirectory(ProjectTempDir);
                }
                LoadConfig();
                DetectSource();
            } catch (Exception ex) {
                //log and ignore?
                Log.Error(ex);
            }
        }

        internal void SetTargetName(string pFileName) {
            TargetPath = CrazyUtils.PathHelper.NormalizeAbsolutePath(Path.Combine(Settings.FinalDirectory, pFileName));
        }

        internal void Reload() {
            try {
                LoadConfig();
            } catch { }
        }

        internal void WriteConfig() {
            if (File.Exists(SourceFile)) {
                FixSourceFile();
                AddConsumer();
            } else {
                Log.Info("Source file missing, not adding consumer to config yet");
                return;
            }

            var locked = false;
            do {
                try {
                    var objTmp = new XDocument(_objConfig);
                    FixLocale(objTmp);
                    File.WriteAllText(ConfigFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine + objTmp.ToString());
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
                } catch (FileNotFoundException ex) {
                    throw ex;
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
            if (string.IsNullOrEmpty(SourceFile)) { return; }

            var objInfo = VideoInfoProvider.Get(SourceFile);
            if (objInfo == null) { return; }

            _dicConsumerProperties = Settings.ConsumerProperties; //get settings from configuration
            _dicConsumerProperties["target"] = TempTargetPath;//Path.Combine(Settings.FinalDirectory, TargetPath);

            //VIDEO
            _dicConsumerProperties["vcodec"] = objInfo.VideoCodec;
            _dicConsumerProperties["width"] = objInfo.Width.ToString();
            _dicConsumerProperties["height"] = objInfo.Height.ToString();
            _dicConsumerProperties["acodec"] = objInfo.AudioCodec;

            //create consumer element
            var objEl = new XElement("consumer");
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
            if (string.IsNullOrEmpty(strSourcePath)) {
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
                    if (string.IsNullOrEmpty(strSourcePath)) {
                        strSourcePath = strFullPath;
                    }
                }
            }
            if (!string.IsNullOrEmpty(strSourcePath)) {
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

        private void FixLocale(XDocument pConfig) {
            var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (sep != ".") {
                var matches = pConfig.Descendants().Where(s => (s.Name == "producer" || s.Name == "tractor") && s.Attributes().Any(a => a.Name == "in" || a.Name == "out")).ToList();
                pConfig.Descendants().Where(s => (s.Name == "producer" || s.Name == "tractor") && s.Attributes().Any(a => a.Name == "in" || a.Name == "out")).ToList().ForEach(r => {
                    r.Attribute("in").Value = r.Attribute("in").Value.Replace(".", sep);
                    r.Attribute("out").Value = r.Attribute("out").Value.Replace(".", sep);
                });
                pConfig.Descendants().Where(s => s.Name == "property" && s.Attribute("name").Value == "length").ToList().ForEach(r => {
                    r.Value = r.Value.Replace(".", sep);
                });
                pConfig.Descendants().Where(s => s.Name == "entry" && s.Attributes().Any(a => a.Name == "in" || a.Name == "out")).ToList().ForEach(r => {
                    r.Attribute("in").Value = r.Attribute("in").Value.Replace(".", sep);
                    r.Attribute("out").Value = r.Attribute("out").Value.Replace(".", sep);
                });
            }
        }
    }
}