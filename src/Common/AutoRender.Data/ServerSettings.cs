namespace AutoRender.Data {

    public class ServerSettings {
        public string FinalDirectory;
        public string LogDirectory;
        public string MeltPath;
        public string NewDirectory;
        public string ProjectDirectory;
        public int Threads;

        public ServerSettings(string pProjectDirectory, string pNewDirectory, string pFinalDirectory, string pLogDirectory, string pMeltPath, int pThreads) {
            FinalDirectory = pFinalDirectory;
            LogDirectory = pLogDirectory;
            MeltPath = pMeltPath;
            NewDirectory = pNewDirectory;
            ProjectDirectory = pProjectDirectory;
            Threads = pThreads;
        }
    }
}