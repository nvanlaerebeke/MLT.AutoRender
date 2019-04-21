namespace CrazyUtils {
    public class Base {
        private static Base _objSelf = null;
        private log4net.ILog _objLog;


        public Base() {
            _objLog = log4net.LogManager.GetLogger(this.GetType());
        }

        protected static log4net.ILog Log { 
            get {
                if (_objSelf == null) {
                    _objSelf = new Base();
                }
                return _objSelf._objLog;
            }
        }
    }
}
