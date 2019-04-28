using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace CrazyUtils {

    public class SingleInstance : IDisposable {
        public bool _hasHandle = false;

        private Mutex _mutex;

        private void InitMutex() {
            string appGuid = GetStringSha256Hash(Assembly.GetExecutingAssembly().FullName);
            try {
            //    appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
            } catch { }
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            _mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        public SingleInstance(int timeOut) {
            InitMutex();
            try {
                if (timeOut < 0) {
                    _hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
                } else {
                    _hasHandle = _mutex.WaitOne(timeOut, false);
                }
                if (_hasHandle == false) {
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
                }
            } catch (AbandonedMutexException) {
                _hasHandle = true;
            }
        }

        public void Dispose() {
            if (_mutex != null) {
                if (_hasHandle)
                    _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }

        internal string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}