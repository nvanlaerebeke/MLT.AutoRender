using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoRender.Lib.Melt;

namespace AutoRender.Lib.Helpers {
    public static class MeltHelper {

        public static List<Melt.StdHandlers.StdHandler> GetHandlers() {
                var lstMeltHandlers = new List<Melt.StdHandlers.StdHandler>();
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace != null && t.Namespace.Equals(typeof(MeltJob).Namespace + ".StdHandlers") && t.IsSubclassOf(typeof(Melt.StdHandlers.StdHandler))).OrderBy(t => t, new HandlerComparer())) {
                    Melt.StdHandlers.StdHandler h = (Melt.StdHandlers.StdHandler)Activator.CreateInstance(t);
                    lstMeltHandlers.Add(h);
                }
            return lstMeltHandlers;
        }

        private class HandlerComparer : IComparer<Type> {
            private List<string> _prioList = new List<string>
            {
                "Progress",
            };

            public int Compare(Type x, Type y) {
                string xName = x.Name;
                string yName = y.Name;
                if (xName == yName) {
                    return 0;
                } else if (_prioList.Contains(xName) && _prioList.Contains(yName)) {
                    return _prioList.IndexOf(xName) < _prioList.IndexOf(yName) ? -1 : 1;
                } else if (_prioList.Contains(xName)) {
                    return -1;
                } else if (_prioList.Contains(yName)) {
                    return 1;
                } else {
                    return xName.CompareTo(yName);
                }
            }
        }
    }
}
