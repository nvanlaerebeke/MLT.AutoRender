using AutoRender.Data;
using AutoRender.Messaging;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace AutoRender {

    public static class MeltConfig {

        public static FileInfo CreateConfig(WorkspaceItem pWorkspaceItem) {
            var strSourceFile = "../" + Settings.NewDirectoryName + "/" + pWorkspaceItem.New.Name;
            var strNewProjectFile = Path.Combine(Settings.ProjectPath, Path.ChangeExtension(pWorkspaceItem.New.Name, ".mlt"));

            if (!Directory.Exists(Settings.ProjectPath)) { throw new Exception("Unable to access " + Settings.ProjectPath); }
            //if(!File.Exists(strSourceFile)) { throw new Exception("Unable to find source file"); }
            if (File.Exists(strNewProjectFile)) { throw new Exception("Project file already exists"); }

            var objSpan = pWorkspaceItem.New.Duration;
            var strDurationStart = "00:00:00.000";
            var strDurationEnd = string.Format("{0}:{1}:{2}.{3}",
                objSpan.Hours.ToString().PadLeft(2, '0'),
                objSpan.Minutes.ToString().PadLeft(2, '0'),
                objSpan.Seconds.ToString().PadLeft(2, '0'),
                objSpan.Milliseconds.ToString().PadLeft(3, '0')
            );

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("mlt",
                    new XAttribute("LC_NUMERIC", "C"),
                    new XAttribute("producer", "main bin"),
                    new XElement("playlist",
                        new XAttribute("id", "main bin"),
                        new XElement("property", new XAttribute("name", "xml_retain")) { Value = "1" }
                    ),
                    new XElement("profile",
                        new XAttribute("description", "automatic")
                    ),
                    //new XAttribute("width", pWorkspaceItem.New.Width),
                    //new XAttribute("height", pWorkspaceItem.New.Height),
                    //new XAttribute("progressive", "1"),
                    //new XAttribute("sample_aspect_num", "1"),
                    //new XAttribute("display_aspect_num", "1"),
                    //new XAttribute("display_aspect_den", "automatic"),
                    //new XAttribute("frame_rate_num", "automatic"),
                    //new XAttribute("frame_rate_den", "automatic"),
                    //new XAttribute("colorspace", "automatic"),
                    //width = "1280" height = "720"
                    //progressive = "1" sample_aspect_num = "1" sample_aspect_den = "1"
                    //display_aspect_num = "1280" display_aspect_den = "720"
                    //frame_rate_num = "401160000" frame_rate_den = "16599583" colorspace = "709" />
                    new XElement("producer",
                        new XAttribute("id", "black"),
                        new XAttribute("in", strDurationStart),
                        new XAttribute("out", strDurationEnd),
                        new XElement("property", new XAttribute("name", "length")) { Value = strDurationEnd },
                        new XElement("property", new XAttribute("name", "eof")) { Value = "pause" },
                        new XElement("property", new XAttribute("name", "resource")) { Value = "black" },
                        new XElement("property", new XAttribute("name", "aspect_ratio")) { Value = "1" },
                        new XElement("property", new XAttribute("name", "mlt_service")) { Value = "color" },
                        new XElement("property", new XAttribute("name", "set.test_audio")) { Value = "0" }
                    ),
                    new XElement("playlist",
                        new XAttribute("id", "background"),
                        new XElement("entry",
                            new XAttribute("producer", "black"),
                            new XAttribute("in", strDurationStart),
                            new XAttribute("out", strDurationEnd)
                        )
                    ),
                    new XElement("producer",
                        new XAttribute("id", "producer0"),
                        new XAttribute("in", strDurationStart),
                        new XAttribute("out", strDurationEnd),
                        new XElement("property", new XAttribute("name", "resource")) { Value = strSourceFile },
                        new XElement("property", new XAttribute("name", "length")) { Value = strDurationEnd },
                        new XElement("property", new XAttribute("name", "eof")) { Value = "pause" },
                        new XElement("property", new XAttribute("name", "audio_index")) { Value = "1" },
                        new XElement("property", new XAttribute("name", "video_index")) { Value = "0" },
                        new XElement("property", new XAttribute("name", "mute_on_pause")) { Value = "0" },
                        new XElement("property", new XAttribute("name", "mlt_service")) { Value = "avformat-novalidate" },
                        new XElement("property", new XAttribute("name", "seekable")) { Value = "1" },
                        new XElement("property", new XAttribute("name", "aspect_ratio")) { Value = "1" },
                        new XElement("property", new XAttribute("name", "ignore_points")) { Value = "0" },
                        new XElement("property", new XAttribute("name", "global_feed")) { Value = "1" }
                    ),
                    new XElement("playlist",
                        new XAttribute("id", "playlist0"),
                        new XElement("entry",
                            new XAttribute("producer", "producer0"),
                            new XAttribute("in", strDurationStart),
                            new XAttribute("out", strDurationEnd)
                        )
                    ),
                    new XElement("tractor",
                        new XAttribute("id", "tractor0"),
                        new XAttribute("global_feed", "1"),
                        new XElement("property", new XAttribute("name", "shotcut")) { Value = "1" },
                        new XElement("track", new XAttribute("producer", "background")),
                        new XElement("track", new XAttribute("producer", "playlist0"))
                    )
                )
            );

            var builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder)) {
                doc.Save(writer);
            }
            File.WriteAllText(strNewProjectFile, builder.ToString().Replace("utf-16", "utf-8"));
            return new FileInfo(strNewProjectFile);
        }
    }
}