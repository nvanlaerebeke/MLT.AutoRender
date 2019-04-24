using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;

namespace AutoRender.Lib.Helpers {

    public class ErrorHandler {
        private readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Start() {
            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;

            //  This AppDomain-wide event provides a mechanism to prevent exception escalation policy (which, by default, terminates the process) from triggering.
            //  Each handler is passed a UnobservedTaskExceptionEventArgs instance, which may be used to examine the exception and to mark it as observed.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; ;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            LogAndExit(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            LogAndExit(e.ExceptionObject as Exception);
        }

        private void LogAndExit(Exception e) {
            Log.Error("Exception detected:");
            List<Exception> lstToLog = new List<Exception>();

            while (e != null) {
                lstToLog.Add(e);

                var objType = e.GetType();
                switch (objType.ToString()) {
                    case "Npgsql.Tls.ClientAlertException":
                        PropertyInfo objDescription = objType.GetProperty("Description");
                        PropertyInfo objExtraInfo = objType.GetProperty("ExtraInfo");
                        if (objDescription != null && objExtraInfo != null) {
                            if (objDescription.GetValue(e).ToString() == "CertificateUnknown") {
                                Log.Error("Postgres TLS certificate not trusted");
                                Log.Error(objExtraInfo.GetValue(e).ToString());
                                Log.Error("Run 'certmgr -add -c -m Trust certificate.crt' and make sure the connection string matches the certificate host name");
                                Environment.Exit(1);
                            }
                        }
                        break;
                }
                e = e.InnerException;
            }
            //log exceptions & exit
            lstToLog.ForEach(ex => { Log.Error(ex); });
            Log.Error("Exiting...");
            Environment.Exit(1);
        }
    }
}