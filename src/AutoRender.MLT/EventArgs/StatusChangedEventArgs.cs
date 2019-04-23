namespace AutoRender.MLT.EventArgs {
    public class StatusChangedEventArgs: System.EventArgs {
        public JobStatus Status { get; private set; }
        
        public StatusChangedEventArgs(JobStatus pStatus) {
            Status = pStatus;
        }
    }
}
