using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CrazyUtils {

    public static class PathHelper {

        public static string NormalizeRelativePath(this String pPath) {
            if (String.IsNullOrEmpty(pPath)) { return string.Empty; }

            // -- use win/unix slashes depending on the OS
            pPath = (Path.DirectorySeparatorChar == '/') ? pPath.Replace('\\', Path.DirectorySeparatorChar) : pPath.Replace('/', Path.DirectorySeparatorChar);

            // -- remove multiple slashes
            pPath = new Regex("~/{2,}~").Replace(pPath, Path.DirectorySeparatorChar.ToString());
            return pPath.Trim(new char[] { Path.DirectorySeparatorChar });
        }

        public static string NormalizeAbsolutePath(this String pPath) {
            // -- use win/unix slashes depending on the OS
            pPath = (Path.DirectorySeparatorChar == '/') ? pPath.Replace('\\', Path.DirectorySeparatorChar) : pPath.Replace('/', Path.DirectorySeparatorChar);

            // -- remove multiple slashes
            pPath = new Regex("~/{2,}~").Replace(pPath, Path.DirectorySeparatorChar.ToString());
            return pPath.TrimEnd(new char[] { Path.DirectorySeparatorChar });
        }

        public static bool FileEquals(FileInfo a, FileInfo b) {
            return (
                a.FullName.Equals(b.FullName) && // -- check if file path is the same
                a.LastWriteTimeUtc.Equals(b.LastWriteTimeUtc) && // -- check if lastmtime is the same
                (a.Exists == b.Exists) && // -- check if they both exist or both do not exist
                (a.Exists == false || a.Length == b.Length) // -- if they exist, check if the length (size) is the same
            );
        }
    }
}