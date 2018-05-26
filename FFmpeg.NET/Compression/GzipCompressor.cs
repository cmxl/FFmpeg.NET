using System.IO;
using System.IO.Compression;

namespace FFmpeg.NET.Compression
{
    public class GzipCompressor : ICompressor
    {
        public void Compress(Stream input, Stream output)
        {
            using (var compressedStream = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(compressedStream);
            }
        }

        public void Decompress(Stream input, Stream output)
        {
            using (var decompressedStream = new GZipStream(input, CompressionMode.Decompress))
            {
                decompressedStream.CopyTo(output);
            }
        }
    }
}