using System.IO;

namespace AutoRender.Workspace.Monitor {

    internal class FSEventInfo {
        public FileSystemEventArgs Args;

        public FSEventInfo(object sender, FileSystemEventArgs e) {
            Args = e as FileSystemEventArgs;
        }
    }
}