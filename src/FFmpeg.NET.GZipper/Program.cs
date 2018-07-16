using System;
using System.IO;
using CommandLine;
using FFmpeg.NET.Compression;

namespace FFmpeg.NET.GZipper
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<GZipOptions>(args)
                .MapResult(
                    RunGzipAndReturnExitCode,
                    errors => 1);
        }

        private static int RunGzipAndReturnExitCode(GZipOptions options)
        {
            try
            {
                PathHelper.CreateDirectoryIfNotExists(options.Output);

                using (var input = new FileStream(options.Input, FileMode.Open, FileAccess.Read))
                using (var output = new FileStream(options.Output, FileMode.Create, FileAccess.ReadWrite))
                {
                    new CompressorFactory().CreateCompressor(CompressorType.GZip).Compress(input, output);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }
}