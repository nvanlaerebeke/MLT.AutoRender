using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutoRender.Lib.Manager {
    public static class ConfigManager {
        private static IniData _objConnectionSettings;
        private static string _strConnectionConfigFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "AutoRender" + Path.DirectorySeparatorChar + "config.ini";
        private static Mutex _objConfigWriteMutex = new Mutex();

        private static IniData Data {
            get {
                if (_objConnectionSettings == null) {
                    if (File.Exists(_strConnectionConfigFile)) {
                        _objConnectionSettings = new FileIniDataParser().ReadFile(_strConnectionConfigFile);
                    } else {
                        _objConnectionSettings = new IniData();
                    }
                }
                return _objConnectionSettings;
            }
        }

        private static List<KeyData> GetValueFromConfig(Section pSection, string pParam, Type pType) {
            List<KeyData> lstValues = new List<KeyData>();
            if (pType.IsGenericType && pType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                if (Data.Sections.ContainsSection(pSection.ToString())) {
                    lstValues.AddRange(Data.Sections[pSection.ToString()]);
                }
            } else {
                if (Data.Sections.ContainsSection(pSection.ToString()) && Data.Sections[pSection.ToString()].ContainsKey(pParam)) {
                    lstValues.Add(Data.Sections[pSection.ToString()].GetKeyData(pParam));
                }
            }
            return lstValues;
        }
        
        private static void WriteValueToConfig<T>(Section pSection, List<KeyData> pValues) {
            //Add section if it does not exist yet
            if (!Data.Sections.ContainsSection(pSection.ToString())) {
                Data.Sections.Add(new SectionData(pSection.ToString()));
            }

            foreach (var obj in pValues) {
                Data.Sections[pSection.ToString()].AddKey(obj);
            }

            try {
                //Create directory if it doesn't exist
                if (!Directory.Exists(Path.GetDirectoryName(_strConnectionConfigFile))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(_strConnectionConfigFile));
                }

                //Save the updated config file
                new FileIniDataParser().WriteFile(_strConnectionConfigFile, Data);
            } catch (Exception ex) {
                Console.WriteLine("Failed to write updated config file " + _strConnectionConfigFile);
                Console.WriteLine(ex);
            }
        }

        private static List<KeyData> GetValues<T>(string pParam, T pParams, List<string> pComments) {
            List<KeyData> lstValues = new List<KeyData>();
            var objType = typeof(T);
            if (objType.IsGenericType && objType.GetInterfaces().Any(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                foreach (var obj in (IEnumerable<KeyValuePair<string, string>>)pParams) {
                    var objKeyData = new KeyData(obj.Key);
                    objKeyData.Value = obj.Value;
                    lstValues.Add(objKeyData);
                }
            } else {
                var objKeyData = new KeyData(pParam);
                objKeyData.Value = pParams.ToString();
                if (pComments != null && pComments.Count > 0) {
                    if (pComments.Count > 1 || !String.IsNullOrEmpty(pComments[0])) {
                        objKeyData.Comments = pComments;
                    }
                }
                lstValues.Add(objKeyData);
            }
            return lstValues;
        }

        public static T Get<T>(Section pSection, string pParam, T pDefault, List<string> pComments) {
            lock (_objConfigWriteMutex) {
                var objType = typeof(T);
                var lstValues = GetValueFromConfig(pSection, pParam, objType);
                if (lstValues.Count == 0) {
                    lstValues = GetValues<T>(pParam, pDefault, pComments);
                    WriteValueToConfig<T>(pSection, lstValues);
                }

                //return the param in the correct type
                switch (objType.Name) {
                    case "Boolean":
                        return (T)(object)Boolean.Parse(lstValues[0].Value);
                    case "Int32":
                    case "Int64":
                        return (T)(object)int.Parse(lstValues[0].Value);
                    case "String":
                        return (T)(object)lstValues[0].Value.Trim(new char[] { '"', '\'' });
                    case "Uri":
                        return (T)(object)new Uri(lstValues[0].Value.Trim(new char[] { '"', '\'' }));
                    case "Dictionary`2":
                        var dicValues = new Dictionary<string, string>();
                        foreach(var obj in lstValues) {
                            dicValues.Add(obj.KeyName, obj.Value);
                        }
                        return (T)(object)dicValues;
                    default:
                        throw new Exception("Unsuported configuration type");
                }
            }
        }
        public static T Get<T>(Section pSection, string pParam, T pDefault, string pComment) {
            return Get<T>(pSection, pParam, pDefault, new List<string> { pComment });
        }
        public static T Get<T>(Section pSection, string pParam, T pDefault) {
            return Get<T>(pSection, pParam, pDefault, "");
        }
        public static T Get<T>(Section pSection, T pDefault)  {
            return Get<T>(pSection, pSection.ToString(), pDefault);
        }
    }
}