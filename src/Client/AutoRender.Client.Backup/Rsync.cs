using System;
using System.IO;
using CrazyUtils;

namespace AutoRender.Client.Backup {
    internal class Rsync : IDisposable {
        public event EventHandler<ProcessStatus> StatusChanged;
        public event EventHandler<string> Progress;

        private readonly Tail Tail;
        private readonly string From;
        private readonly string To;
        private readonly string LogFile;
        private readonly BashFile BashFile;

        private ProcessRunner Process;

        public Rsync(string pFrom, string pTo) {
            From = pFrom;
            To = pTo;
            LogFile = Path.Combine(From, "logs", $"backup_{ string.Format("{0:s}", DateTime.Now)}.log".Replace(':', '-'));

            BashFile = new BashFile(From, To, LogFile);

            Tail = new Tail(LogFile, 10000);
            Tail.Changed += Tail_Changed;
        }

        public void Start() {
            if (!Directory.Exists(Path.GetDirectoryName(LogFile))) {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(LogFile));
            }

            if (!File.Exists(LogFile)) {
                using (var stream = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.Read)) { }
            }

            var bashfile = BashFile.Create();
            var command = $@"bash.exe -c '{Cygwin.GetCygwinPath(bashfile)}'";

            Process = new ProcessRunner("bash.exe", $"-c '{Cygwin.GetCygwinPath(bashfile)}'");
            Process.StatusChanged += Process_StatusChanged;

            Tail.Run();

            File.WriteAllText(LogFile, $@"Command:
{command}

Rsync Command:
{File.ReadAllText(bashfile)}".Replace(System.Environment.NewLine, "\n")
            );

            Process.Start();
        }

        private void Tail_Changed(object sender, Tail.TailEventArgs e) {
            Progress?.Invoke(this, e.Line);
        }

        private void Process_StatusChanged(object sender, ProcessStatus e) {
            StatusChanged?.Invoke(this, e);
        }

        public void Dispose() {
            Tail.Changed -= Tail_Changed;
            Process.StatusChanged -= Process_StatusChanged;
        }
    }
}
