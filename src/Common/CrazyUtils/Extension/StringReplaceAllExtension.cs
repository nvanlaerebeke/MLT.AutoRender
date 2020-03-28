using System.Collections.Generic;

namespace CrazyUtils.Extension {

    public static class StringExtension {

        public static string ReplaceAll(this string s, Dictionary<string, string> pReplacements) {
            foreach (var to_replace in pReplacements.Keys) {
                s = s.Replace(to_replace, pReplacements[to_replace]);
            }
            return s;
        }
    }
}