using System.Net;

namespace AutoRender.ServiceTest {
    public class Settings {
        private static Settings _objSettings;

        public static string Server {
            get { return "test.crazyzone.be"; }
        }
        public static int Port { get { return 6666; } }
    }
}
