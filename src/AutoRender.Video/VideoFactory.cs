using AutoRender.Data;
using System;
using System.Diagnostics;
using System.IO;

namespace AutoRender.Video {

    public class VideoFactory {

        public static void Setup() {
            //Verify Settings
            if (!File.Exists(Settings.FfprobePath)) { throw new Exception("FFProbe not found: " + Settings.FfprobePath); }
            if (!File.Exists(Settings.MeltPath)) { throw new Exception("Melt not found: " + Settings.MeltPath); }

            //Setup Environment
            Process objProcess = new Process();
            ProcessStartInfo objStartInfo = new ProcessStartInfo(Settings.SourcePath, Path.GetDirectoryName(Settings.SourcePath)) {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                WorkingDirectory = Settings.TempDirectory
            };

            objProcess.StartInfo = objStartInfo;
            objProcess.Start();
        }
    }
}