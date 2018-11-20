using FFmpeg.NET.Events;
using System;
using System.Threading.Tasks;

namespace FFmpeg.NET.Sample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var inputFile = new MediaFile(@"..\..\..\..\..\tests\FFmpeg.NET.Tests\MediaFiles\SampleVideo_1280x720_1mb.mp4");
                var outputFile = new MediaFile(@"output.mkv");
                var thumbNailFile = new MediaFile(@"thumb.png");

                var ffmpeg = new Engine(@"..\..\..\..\..\lib\ffmpeg\v4\ffmpeg.exe");
                ffmpeg.Progress += OnProgress;
                ffmpeg.Data += OnData;
                ffmpeg.Error += OnError;
                ffmpeg.Complete += OnComplete;
                var output = await ffmpeg.ConvertAsync(inputFile, outputFile);
                var thumbNail = await ffmpeg.GetThumbnailAsync(output, thumbNailFile, new ConversionOptions { Seek = TimeSpan.FromSeconds(3) });
                var metadata = await ffmpeg.GetMetaDataAsync(output);
                Console.WriteLine(metadata.FileInfo.FullName);
                Console.WriteLine(metadata);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void OnProgress(object sender, ConversionProgressEventArgs e)
        {
            Console.WriteLine("[{0} => {1}]", e.Input.FileInfo.Name, e.Output?.FileInfo.Name);
            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("Size: {0} kb", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
        }

        private static void OnData(object sender, ConversionDataEventArgs e)
            => Console.WriteLine("[{0} => {1}]: {2}", e.Input.FileInfo.Name, e.Output?.FileInfo.Name, e.Data);

        private static void OnComplete(object sender, ConversionCompleteEventArgs e)
            => Console.WriteLine("Completed conversion from {0} to {1}", e.Input.FileInfo.FullName, e.Output?.FileInfo.FullName);

        private static void OnError(object sender, ConversionErrorEventArgs e)
            => Console.WriteLine("[{0} => {1}]: Error: {2}\n{3}", e.Input.FileInfo.Name, e.Output?.FileInfo.Name, e.Exception.ExitCode, e.Exception.InnerException);
    }
}
