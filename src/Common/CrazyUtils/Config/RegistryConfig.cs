using System;
using Microsoft.Win32;

namespace CrazyUtils.Config {

    public class RegistryConfig {
        protected static string RegistryROOT { get; set; } = "HKCU";
        protected static string ApplicationName { get; set; }

        protected static T Get<T>(string pKey, T pDefaultValue) {
            object objValue;
            switch (RegistryROOT) {
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    objValue = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey(ApplicationName).GetValue(pKey, null);
                    break;

                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    objValue = Registry.LocalMachine.OpenSubKey("Software", true).CreateSubKey(ApplicationName).GetValue(pKey, null);
                    break;

                default:
                    throw new NotSupportedException($"{RegistryROOT} is not a supported");
            }
            if (objValue == null) {
                Set(pKey, pDefaultValue);
                return pDefaultValue;
            }
            return (T)objValue;
        }

        protected static void Set<T>(string pKey, T pValue) {
            if (pValue == null) {
                Remove(pKey);
                return;
            }
            switch (RegistryROOT) {
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey(ApplicationName).SetValue(pKey, pValue);
                    break;

                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    Registry.LocalMachine.OpenSubKey("Software", true).CreateSubKey(ApplicationName).SetValue(pKey, pValue);
                    break;

                default:
                    throw new NotSupportedException(RegistryROOT + " is not supported");
            }
        }

        protected static void Remove(string pKey) {
            switch (RegistryROOT) {
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey(ApplicationName).DeleteValue(pKey);
                    break;

                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    Registry.LocalMachine.OpenSubKey("Software", true).CreateSubKey(ApplicationName).DeleteValue(pKey);
                    break;

                default:
                    throw new NotSupportedException(RegistryROOT + " is not supported");
            }
        }
    }
}