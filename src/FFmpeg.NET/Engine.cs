using FFmpeg.NET.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET.Extensions;

namespace FFmpeg.NET
{
    public sealed class Engine
    {
        private readonly string _ffmpegPath;

        /// <summary>
        /// Instantiate the FFmpeg engine by providing either the name of the executable or the path to the executable. If only file name is provided, it must be found through the PATH variables.
        /// </summary>
        /// <param name="ffmpegPath">The path to the ffmpeg executable, or the executable if it is defined in PATH. If left empty, it will try to find "ffmpeg.exe" from PATH.</param>
        public Engine(string ffmpegPath = null)
        {
            ffmpegPath = ffmpegPath ?? "ffmpeg.exe";

            if (!ffmpegPath.TryGetFullPath(out _ffmpegPath))
                throw new ArgumentException(ffmpegPath, "FFmpeg executable could not be found neither in PATH nor in directory.");
        }

        public event EventHandler<ConversionProgressEventArgs> Progress;
        public event EventHandler<ConversionErrorEventArgs> Error;
        public event EventHandler<ConversionCompleteEventArgs> Complete;
        public event EventHandler<ConversionDataEventArgs> Data;

        public async Task<MetaData> GetMetaDataAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.GetMetaData,
                InputFile = mediaFile
            };

            await ExecuteAsync(parameters, cancellationToken);
            return parameters.InputFile.MetaData;
        }

        public async Task<MediaFile> GetThumbnailAsync(MediaFile input, MediaFile output, CancellationToken cancellationToken = default)
            => await GetThumbnailAsync(input, output, default, cancellationToken);

        public async Task<MediaFile> GetThumbnailAsync(MediaFile input, MediaFile output, ConversionOptions options, CancellationToken cancellationToken = default)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.GetThumbnail,
                InputFile = input,
                OutputFile = output,
                ConversionOptions = options
            };

            await ExecuteAsync(parameters, cancellationToken);
            return parameters.OutputFile;
        }

        public async Task<MediaFile> ConvertAsync(MediaFile input, MediaFile output, CancellationToken cancellationToken = default)
            => await ConvertAsync(input, output, default, cancellationToken);

        public async Task<MediaFile> ConvertAsync(MediaFile input, MediaFile output, ConversionOptions options, CancellationToken cancellationToken = default)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Convert,
                InputFile = input,
                OutputFile = output,
                ConversionOptions = options
            };

            await ExecuteAsync(parameters, cancellationToken);
            return parameters.OutputFile;
        }

        private async Task ExecuteAsync(FFmpegParameters parameters, CancellationToken cancellationToken = default)
        {
            var ffmpegProcess = new FFmpegProcess(parameters, _ffmpegPath, cancellationToken);
            ffmpegProcess.Progress += OnProgress;
            ffmpegProcess.Completed += OnComplete;
            ffmpegProcess.Error += OnError;
            ffmpegProcess.Data += OnData;
            await ffmpegProcess.ExecuteAsync();

            ffmpegProcess.Progress -= OnProgress;
            ffmpegProcess.Completed -= OnComplete;
            ffmpegProcess.Error -= OnError;
            ffmpegProcess.Data -= OnData;

        }

        public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
        {
            var parameters = new FFmpegParameters { CustomArguments = arguments };
            await ExecuteAsync(parameters, cancellationToken);
        }

        private void OnProgress(ConversionProgressEventArgs e) => Progress?.Invoke(this, e);

        private void OnError(ConversionErrorEventArgs e) => Error?.Invoke(this, e);

        private void OnComplete(ConversionCompleteEventArgs e) => Complete?.Invoke(this, e);

        private void OnData(ConversionDataEventArgs e) => Data?.Invoke(this, e);

    }
}