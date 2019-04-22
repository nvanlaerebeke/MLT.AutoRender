using Mitto.ILogging;
using System;

namespace AutoRender.Logging {

    public static class LogFactory {
        private static LogProvider Provider;

        public static void Initialize() {
            Provider = new LogProvider();
        }

        public static ILog GetLogger(Type pType) {
            return Provider.GetLogger(pType);
        }

        public static ILog GetLogger() {
            return Provider.GetLogger();
        }
    }
}