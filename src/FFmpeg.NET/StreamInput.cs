using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    public class StreamInput : IInputArgument, IProcessExecutionHandler, IDisposable, IAsyncDisposable
    {
        private const int ChannelClosedHResult = -2147024787;
        private const int ReadBufferSize = 64 * 1024; // 64Kb

        public bool UseStandardInput => true;
        public string Argument => "-";
        public string Name => "stream";
        public MetaData MetaData { get; set; }

        private readonly Stream _stream;
        private readonly bool _disposeStream;

        public StreamInput(Stream stream, bool disposeStream = false)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("Input stream should be readable!");
            }

            _stream = stream;
            _disposeStream = disposeStream;
        }

        public Task HandleProcessExitedAsync(Process process, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleProcessStartedAsync(Process process, CancellationToken cancellationToken)
        {
            if (process is null || process.HasExited)
            {
                return;
            }

            try
            {
                Stream inputStream = process.StandardInput.BaseStream;
                byte[] buffer = new byte[ReadBufferSize];
                int bytesRead = 0;

                do
                {
                    bytesRead = await _stream.ReadAsync(buffer, 0, ReadBufferSize, cancellationToken)
                        .ConfigureAwait(false);

                    await inputStream.WriteAsync(buffer, 0, bytesRead, cancellationToken)
                        .ConfigureAwait(false);

                } while (bytesRead > 0 && !process.HasExited);
            }
            catch (IOException ioException) when (ioException.HResult == ChannelClosedHResult)
            {
                // If input stream has already closed, ignore it.
            }
            catch (InvalidOperationException)
            {
                // If the process doesn't exist anymore, ignore it.
            }

            Close(process);
        }

        public void Dispose()
        {
            if (_disposeStream)
            {
                _stream.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposeStream)
            {
                await _stream.DisposeAsync();
            }
        }

        private static void Close(Process process)
        {
            if (process is null || process.HasExited)
            {
                return;
            }
            try
            {
                process.StandardInput.Close();
            }
            catch (InvalidOperationException)
            {
                // If the process doesn't exist anymore, ignore it.
            }
        }
    }
}