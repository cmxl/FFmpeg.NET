using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    public class StandardInputWriter : IInputArgument, IProcessExecutionHandler
    {
        private const int ChannelClosedHResult = -2147024787;

        public bool UseStandardInput => true;
        public string Argument => "-";
        public string Name => "stdin";
        public MetaData MetaData { get; set; }

        private volatile Process _process;

        public bool IsOpen => _process?.HasExited == false;

        public Task HandleProcessStartedAsync(Process process, CancellationToken cancellationToken)
        {
            _process = process;
            return Task.CompletedTask;
        }

        public Task HandleProcessExitedAsync(Process process, CancellationToken cancellationToken)
        {
            _process = null;
            return Task.CompletedTask;
        }

        public async Task WriteAsync(byte[] data, int offset, int count, CancellationToken cancellationToken = default)
        {
            await WriteAsync(new ReadOnlyMemory<byte>(data, offset, count), cancellationToken).ConfigureAwait(false);
        }

        public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            if (_process is null || _process.HasExited)
            {
                return;
            }

            try
            {
                var inputStream = _process.StandardInput.BaseStream;
                await inputStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException ioException) when (ioException.HResult == ChannelClosedHResult)
            {
                // If input stream has already closed, ignore it.
            }
            catch (InvalidOperationException)
            {
                // If the process doesn't exist anymore, ignore it.
            }
        }

        public void Close()
        {
            if (_process is null || _process.HasExited)
            {
                return;
            }
            try
            {
                _process.StandardInput.Close();
            }
            catch (InvalidOperationException)
            {
                // If the process doesn't exist anymore, ignore it.
            }
        }
    }
}