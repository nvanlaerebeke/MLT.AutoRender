using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AutoRender.Lib.Helpers {
    public class ErrorHelper : CrazyUtils.Base {
        public static void Init() {

            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;

            //  This AppDomain-wide event provides a mechanism to prevent exception escalation policy (which, by default, terminates the process) from triggering.
            //  Each handler is passed a UnobservedTaskExceptionEventArgs instance, which may be used to examine the exception and to mark it as observed.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; ;
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            LogAndExit(e.Exception);
        }


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            LogAndExit(e.ExceptionObject as Exception);
        }

        static void LogAndExit(Exception e) {
            Log.Error("Unhandled Exception detected:");
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
                                Log.Error("Postgres tls certificate not trusted");
                                Log.Error(objExtraInfo.GetValue(e).ToString());
                                Log.Error("Run 'certmgr -add -c -m Trust certificate.crt' and make sure the connectionstring matches the certificate hostname");
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