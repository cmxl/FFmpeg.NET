namespace FFmpeg.NET
{
    public class FFmpegParameters
    {
        public bool HasCustomArguments => !string.IsNullOrWhiteSpace(CustomArguments);
        public string CustomArguments { get; set; }
        public ConversionOptions ConversionOptions { get; set; }
        public FFmpegTask Task { get; set; }
        public IOutputArgument Output { get; set; }
        public IInputArgument Input { get; set; }
    }
}