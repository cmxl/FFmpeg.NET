namespace FFmpeg.NET
{
    internal class FFmpegParameters
    {
        internal bool HasCustomArguments => !string.IsNullOrWhiteSpace(CustomArguments);
        internal string CustomArguments { get; set; }
        internal ConversionOptions ConversionOptions { get; set; }
        internal FFmpegTask Task { get; set; }
        internal MediaFile OutputFile { get; set; }
        internal MediaFile InputFile { get; set; }
    }
}