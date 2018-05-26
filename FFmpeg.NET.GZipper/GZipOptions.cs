using CommandLine;

namespace FFmpeg.NET.GZipper
{
    [Verb("gzip", HelpText = "GZip a source file to a destination file")]
    public class GZipOptions
    {
        [Option('i', "input", Required = true, HelpText = @"File to be gzipped. (e.g. C:\Temp\foo.txt)")]
        public string Input { get; set; }

        [Option('o', "output", Required = true, HelpText = @"Output path for gzipped file. (e.g. C:\Temp\foo.txt.gz)")]
        public string Output { get; set; }
    }
}