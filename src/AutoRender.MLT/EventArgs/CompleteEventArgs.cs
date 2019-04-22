namespace AutoRender.MLT.EventArgs {

    public class CompleteEventArgs: System.EventArgs {
        public int ExitCode { get; private set; }
        public double TotalSeconds { get; private set; }

        public CompleteEventArgs(int pExitCode, double pTime) {
            ExitCode = pExitCode;
            TotalSeconds = pTime;
        }
    }
}