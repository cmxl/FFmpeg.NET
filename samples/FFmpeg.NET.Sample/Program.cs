using FFmpeg.NET.Enums;
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
                // capture desktop with ffmpeg
                // ffmpeg -y -f gdigrab -framerate 10 -video_size 1920x1080 -i desktop output.mp4

                var inputFile = new InputFile(@"..\..\..\..\..\tests\FFmpeg.NET.Tests\MediaFiles\SampleVideo_1280x720_1mb.mp4");
                var outputFile = new OutputFile(@"output.mkv");
                var thumbNailFile = new OutputFile(@"thumb.png");

                var ffmpeg = new Engine(@"..\..\..\..\..\lib\ffmpeg\v4\ffmpeg.exe");
                ffmpeg.Progress += OnProgress;
                ffmpeg.Data += OnData;
                ffmpeg.Error += OnError;
                ffmpeg.Complete += OnComplete;
                var output = await ffmpeg.ConvertAsync(inputFile, outputFile);
                var thumbNail = await ffmpeg.GetThumbnailAsync(new InputFile(output.FileInfo.FullName), thumbNailFile,
                    new ConversionOptions
                    {
                        Seek = TimeSpan.FromSeconds(3),
                        VideoSize = Enums.VideoSize.Custom,
                        SourceCrop = new CropRectangle { X = 100, Width = 200, Y = 100, Height = 100 }
                    });
                var metadata = await ffmpeg.GetMetaDataAsync(new InputFile(output.FileInfo.FullName));
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
            Console.WriteLine("[{0} => {1}]", e.Input.Name, e.Output?.Name);
            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("Size: {0} kb", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
        }

        private static void OnData(object sender, ConversionDataEventArgs e)
            => Console.WriteLine("[{0} => {1}]: {2}", e.Input.Name, e.Output?.Name, e.Data);

        private static void OnComplete(object sender, ConversionCompleteEventArgs e)
            => Console.WriteLine("Completed conversion from {0} to {1}", e.Input.Name, e.Output?.Name);

        private static void OnError(object sender, ConversionErrorEventArgs e)
            => Console.WriteLine("[{0} => {1}]: Error: {2}\n{3}", e.Input.Name, e.Output?.Name, e.Exception.ExitCode, e.Exception.InnerException);
    }
}
