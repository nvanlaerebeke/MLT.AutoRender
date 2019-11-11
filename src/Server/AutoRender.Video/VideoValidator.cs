using AutoRender.Data;
using CrazyUtils;
using log4net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AutoRender.Video {

    public class VideoValidator {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ProcessRunner Process;

        private bool Valid { get; set; } = false;
        private ManualResetEvent Blocker = new ManualResetEvent(false);

        public VideoValidator(string pPath) {
            Process = new ProcessRunner(Settings.FfmpegPath, "-i \"" + pPath + "\"");
            Log.Info($"Running: {Settings.FfmpegPath} -show_streams -i \"{pPath}\"");
        }

        public Task<bool> IsValid() {
            return Task.Run(() => {
                Process.StatusChanged += Process_StatusChanged;
                Process.StdOut += Process_StdOut;
                Process.Start();

                Blocker.WaitOne();

                Process.StatusChanged -= Process_StatusChanged;
                Process.StdOut -= Process_StdOut;

                return Valid;
            });
        }

        void Process_StatusChanged(object sender, ProcessStatus e) {
            switch(e) {
                case ProcessStatus.Done:
                case ProcessStatus.Failed:
                case ProcessStatus.Stopped:
                    Blocker.Set();
                    break;
                case ProcessStatus.Paused:
                case ProcessStatus.Running:
                    break;
            }
        }

        void Process_StdOut(object sender, string e) {
            Log.Info(e);
            if(e.Contains("Invalid data found when processing input")) {
                Valid = false;
            }
            if(e.Contains("At least one output file must be specified")) {
                Valid = true;
            }
        }
    }
}