using AutoRender.Data;
using System;
using System.IO;

namespace AutoRender.Video {

    public static class VideoFactory {

        public static void Setup() {
            //Verify Settings
            if (!File.Exists(Settings.FfprobePath)) { throw new Exception("FFProbe not found: " + Settings.FfprobePath); }
            if (!File.Exists(Settings.MeltPath)) { throw new Exception("Melt not found: " + Settings.MeltPath); }
        }
    }
}