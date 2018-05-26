using System;

namespace FFmpeg.NET.Compression
{
    public class CompressorFactory
    {
        public ICompressor CreateCompressor(CompressorType compressorType)
        {
            switch (compressorType)
            {
                case CompressorType.GZip:
                    return new GzipCompressor();

                default:
                    throw new NotImplementedException($"CompressorType {compressorType} has not been implemented.");
            }
        }
    }
}