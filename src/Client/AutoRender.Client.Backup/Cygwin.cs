using System.IO;

namespace AutoRender.Client.Backup {
    internal static class Cygwin {
        public static string GetCygwinPath(string pPath) {
            var basepath = pPath.Substring(Path.GetPathRoot(pPath).Length);
            basepath = basepath.Replace("\\", "/");
            return $"/cygdrive/{Path.GetPathRoot(pPath).ToLower()[0]}/{basepath}";
        }
    }
}
