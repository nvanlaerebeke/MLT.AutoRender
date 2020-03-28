using System.Reflection;
using log4net;

namespace AutoRender.Server.Services {

    internal abstract class Service : IService {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract void Start();

        public abstract void Stop();

        public virtual byte Priority {
            get { return 1; }
        }
    }
}