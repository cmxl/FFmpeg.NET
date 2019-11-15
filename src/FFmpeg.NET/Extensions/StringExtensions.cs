using System;
using System.IO;

namespace FFmpeg.NET.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check if the string value equals a path to a file using File.Exists
        /// </summary>
        /// <param name="filePath">The full path to the file to check</param>
        /// <param name="fullPath">The verified full path. If file does not exist, it returns string.Empty.</param>
        /// <returns></returns>
        public static bool TryGetFullPathIfFileExists(this string filePath, out string fullPath)
        {
            fullPath = string.Empty;

            if (!File.Exists(filePath)) return false;

            fullPath = Path.GetFullPath(filePath);
            return true;
        }

        /// <summary>
        /// Check if the string value equals a path to a file using the PATH environment variables
        /// </summary>
        /// <param name="fileName">The filename to check if exists in path variables</param>
        /// <param name="fullPath">The verified full path. If file does not exist, it returns string.Empty.</param>
        /// <returns></returns>
        public static bool TryGetFullPathIfPathEnvironmentExists(this string fileName, out string fullPath)
        {
            fullPath = string.Empty;
            var values = Environment.GetEnvironmentVariable("PATH");
            var pathElements = values?.Split(Path.PathSeparator);

            if (pathElements == null) return false;

            foreach (var path in pathElements)
            {
                var tempFullPath = Path.Combine(path, fileName);
                if (tempFullPath.TryGetFullPathIfFileExists(out fullPath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a file or file path exists or if the file can be found through the PATH variables.
        /// </summary>
        /// <param name="file">The file name or file path to check</param>
        /// <param name="fullPath">The verified full path. If file does not exist, it returns string.Empty.</param>
        /// <returns></returns>
        public static bool TryGetFullPath(this string file, out string fullPath)
        {
            if (file.TryGetFullPathIfFileExists(out fullPath)) return true;
            if (file.TryGetFullPathIfPathEnvironmentExists(out fullPath)) return true;

            return false;
        }
    }
}
