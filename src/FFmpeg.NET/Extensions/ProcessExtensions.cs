using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET.Extensions
{
    public static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process, bool stdInDataInput, Action<int> onException, CancellationToken cancellationToken = default)
        {
            CancellationTokenRegistration ctRegistration = new CancellationTokenRegistration();
            bool mustUnregister = false;
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            if (cancellationToken != default)
            {
                mustUnregister = true;
                ctRegistration = cancellationToken.Register(() =>
                {
                    try
                    {
                        if (stdInDataInput)
                        {
                            // If standard input used for data input just close it.
                            // It will force process to stop (closing files).
                            process.StandardInput.Close();
                        }
                        else
                        {
                            // Send "q" to ffmpeg, which will force it to stop (closing files).
                            process.StandardInput.Write("q");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // If the process doesn't exist anymore, ignore it.
                    }
                    finally
                    {
                        // Cancel the task. This will throw an exception to the calling program.
                        // Exc.Message will be "A task was canceled."
                        try
                        {
                            tcs.SetCanceled();
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
            }

            void processOnExited(object sender, EventArgs e)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    onException?.Invoke(process.ExitCode);
                tcs.TrySetResult(process.ExitCode);
                if (mustUnregister) ctRegistration.Dispose();
                process.Exited -= processOnExited;
            }

            process.EnableRaisingEvents = true;
            process.Exited += processOnExited;

            var started = process.Start();
            if (!started)
                tcs.TrySetException(new InvalidOperationException($"Could not start process {process}"));

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}