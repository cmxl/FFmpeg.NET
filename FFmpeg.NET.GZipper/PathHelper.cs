using System.IO;

namespace FFmpeg.NET.GZipper
{
    internal static class PathHelper
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);
            }
            catch
            {
            }
        }
    }
}