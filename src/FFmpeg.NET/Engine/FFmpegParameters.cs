namespace FFmpeg.NET.Engine
{
    internal class FFmpegParameters
    {
        internal ConversionOptions ConversionOptions { get; set; }
        internal FFmpegTask Task { get; set; }
        internal MediaFile OutputFile { get; set; }
        internal MediaFile InputFile { get; set; }
    }
}