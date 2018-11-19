using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET.Extensions
{
    public static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process, Action<int> onException, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<int>();
            if (cancellationToken != default)
                cancellationToken.Register(tcs.SetCanceled);

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                if (process.ExitCode != 0)
                    onException?.Invoke(process.ExitCode);
                tcs.TrySetResult(process.ExitCode);
            };

            var started = process.Start();
            if (!started)
                tcs.TrySetException(new InvalidOperationException($"Could not start process {process}"));

            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}
