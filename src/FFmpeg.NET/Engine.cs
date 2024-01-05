using FFmpeg.NET.Events;
using FFmpeg.NET.Extensions;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    public sealed class Engine
    {
        private readonly string _ffmpegPath;
        private readonly string _pipePrefix = "xffmpegnet_";

        /// <summary>
        /// Instantiate the FFmpeg engine by providing either the name of the executable or the path to the executable. If only file name is provided, it must be found through the PATH variables.
        /// </summary>
        /// <param name="ffmpegPath">The path to the ffmpeg executable, or the executable if it is defined in PATH. If left empty, it will try to find "ffmpeg.exe" from PATH.</param>
        public Engine(string ffmpegPath = null)
        {
            ffmpegPath ??= "ffmpeg.exe";

            if (!ffmpegPath.TryGetFullPath(out _ffmpegPath))
                throw new ArgumentException("FFmpeg executable could not be found neither in PATH nor in directory.", ffmpegPath);
        }

        public event EventHandler<ConversionProgressEventArgs> Progress;
        public event EventHandler<ConversionErrorEventArgs> Error;
        public event EventHandler<ConversionCompleteEventArgs> Complete;
        public event EventHandler<ConversionDataEventArgs> Data;


        // ---------------------------------------------------------
        // Wrapper methods for ease of use
        // ---------------------------------------------------------

        public async Task<MetaData> GetMetaDataAsync(IInputArgument mediaFile, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.GetMetaData,
                Input = mediaFile
            };

            await ExecuteAsync(parameters, cancellationToken).ConfigureAwait(false);
            return mediaFile.MetaData;
        }

        public async Task<MediaFile> GetThumbnailAsync(IInputArgument input, OutputFile output, CancellationToken cancellationToken)
            => await GetThumbnailAsync(input, output, default, cancellationToken).ConfigureAwait(false);

        public async Task<MediaFile> GetThumbnailAsync(IInputArgument input, OutputFile output, ConversionOptions options, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.GetThumbnail,
                Input = input,
                Output = output,
                ConversionOptions = options
            };

            await ExecuteAsync(parameters, cancellationToken).ConfigureAwait(false);
            return output;
        }

        public async Task<MediaFile> ConvertAsync(IInputArgument input, OutputFile output, CancellationToken cancellationToken)
            => await ConvertAsync(input, output, default, cancellationToken).ConfigureAwait(false);

        public async Task<MediaFile> ConvertAsync(IInputArgument input, OutputFile output, ConversionOptions options, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Convert,
                Input = input,
                Output = output,
                ConversionOptions = options
            };

            await ExecuteAsync(parameters, cancellationToken).ConfigureAwait(false);
            return output;
        }

        public async Task<Stream> ConvertAsync(IInputArgument input, ConversionOptions options, CancellationToken cancellationToken)
        {
            var ms = new MemoryStream();
            await ConvertAsync(input, ms, options, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }

        public async Task ConvertAsync(IInputArgument input, Stream output, ConversionOptions options, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Convert,
                Input = input,
                ConversionOptions = options
            };
            await ExecuteAsync(parameters, output, cancellationToken).ConfigureAwait(false);
        }

        public async Task ConvertAsync(IArgument argument, Stream output, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Execute,
                CustomArguments = argument.Argument
            };
            await ExecuteAsync(parameters, output, cancellationToken).ConfigureAwait(false);
        }

        public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Execute,
                CustomArguments = arguments,
            };
            await ExecuteAsync(parameters, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task ExecuteAsync(string arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            var parameters = new FFmpegParameters
            {
                Task = FFmpegTask.Execute,
                CustomArguments = arguments,
                WorkingDirectory = workingDirectory
            };
            await ExecuteAsync(parameters, cancellationToken).ConfigureAwait(false);
        }

        // ---------------------------------------------------------
        // Basic API to call ffmpeg
        // ---------------------------------------------------------

        public async Task ExecuteAsync(FFmpegParameters parameters, CancellationToken cancellationToken)
        {
            var ffmpegProcess = CreateProcess(parameters);
            await ffmpegProcess.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            Cleanup(ffmpegProcess);
        }

        public async Task ExecuteAsync(FFmpegParameters parameters, Stream output, CancellationToken cancellationToken)
        {
            var outputPipeName = $"{_pipePrefix}{Guid.NewGuid()}";
            var outputArgument = new OutputPipe(GetPipePath(outputPipeName));
            var pipe = new NamedPipeServerStream(outputPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            parameters.Output = outputArgument;
            var ffmpegProcess = CreateProcess(parameters);
            try
            {
                var executeProcess = ffmpegProcess.ExecuteAsync(cancellationToken);
                var copyData = pipe.WaitForConnectionAsync(cancellationToken)
                    .ContinueWith(async x =>
                    {
                        await pipe.CopyToAsync(output, cancellationToken);
                    }, cancellationToken).Unwrap();
                await Task.WhenAll(executeProcess, copyData).ConfigureAwait(false);
                pipe.Disconnect();
            }
            finally
            {
                pipe.Dispose();
                Cleanup(ffmpegProcess);
            }
        }

        private FFmpegProcess CreateProcess(FFmpegParameters parameters)
        {
            var ffmpegProcess = new FFmpegProcess(parameters, _ffmpegPath);
            ffmpegProcess.Progress += OnProgress;
            ffmpegProcess.Completed += OnComplete;
            ffmpegProcess.Error += OnError;
            ffmpegProcess.Data += OnData;
            return ffmpegProcess;
        }

        private void Cleanup(FFmpegProcess ffmpegProcess)
        {
            ffmpegProcess.Progress -= OnProgress;
            ffmpegProcess.Completed -= OnComplete;
            ffmpegProcess.Error -= OnError;
            ffmpegProcess.Data -= OnData;
        }

        private static string GetPipePath(string pipeName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"\\.\pipe\{pipeName}";
            else
                return $"unix:/tmp/CoreFxPipe_{pipeName}";
        }

        private void OnProgress(ConversionProgressEventArgs e) => Progress?.Invoke(this, e);
        private void OnError(ConversionErrorEventArgs e) => Error?.Invoke(this, e);
        private void OnComplete(ConversionCompleteEventArgs e) => Complete?.Invoke(this, e);
        private void OnData(ConversionDataEventArgs e) => Data?.Invoke(this, e);

    }
}