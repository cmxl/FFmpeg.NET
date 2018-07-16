using System.IO;

namespace FFmpeg.NET.Compression
{
    public interface ICompressor
    {
        void Compress(Stream input, Stream output);
        void Decompress(Stream input, Stream output);
    }
}