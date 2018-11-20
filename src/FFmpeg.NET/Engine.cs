using FFmpeg.NET.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    public sealed class Engine
    {
        private readonly string _ffmpegPath;

        public Engine(string ffmpegPath)
        {
            _ffmpegPath = ffmpegPath ?? throw new ArgumentNullException(ffmpegPath, "FFmpeg executable path needs to be provided.");
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
            var ffmpegProcess = new FFmpegProcess();
            ffmpegProcess.Progress += OnProgress;
            ffmpegProcess.Completed += OnComplete;
            ffmpegProcess.Error += OnError;
            ffmpegProcess.Data += OnData;
            await ffmpegProcess.ExecuteAsync(parameters, _ffmpegPath, cancellationToken);
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