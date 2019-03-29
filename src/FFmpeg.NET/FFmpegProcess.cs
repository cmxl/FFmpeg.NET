using FFmpeg.NET.Events;
using FFmpeg.NET.Exceptions;
using FFmpeg.NET.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    internal sealed class FFmpegProcess
    {
        public async Task ExecuteAsync(FFmpegParameters parameters, string ffmpegFilePath, CancellationToken cancellationToken = default)
        {
            var argumentBuilder = new FFmpegArgumentBuilder();
            var arguments = argumentBuilder.Build(parameters);
            var startInfo = GenerateStartInfo(ffmpegFilePath, arguments);
            await ExecuteAsync(startInfo, parameters, cancellationToken);
        }

        private async Task ExecuteAsync(ProcessStartInfo startInfo, FFmpegParameters parameters, CancellationToken cancellationToken = default)
        {
            var messages = new List<string>();
            Exception caughtException = null;

            using (var ffmpegProcess = new Process() { StartInfo = startInfo })
            {
                ffmpegProcess.ErrorDataReceived += (sender, e) => OnData(new ConversionDataEventArgs(e.Data, parameters.InputFile, parameters.OutputFile));
                ffmpegProcess.ErrorDataReceived += (sender, e) => FFmpegProcessOnErrorDataReceived(e, parameters, ref caughtException, messages);

                Task<int> task = null;
                try
                {
                    task = ffmpegProcess.WaitForExitAsync((exitCode) => OnException(messages, parameters, exitCode, caughtException), cancellationToken);
                    await task;
                }
                catch (Exception)
                {
                    // An exception occurs if the user cancels the operation by calling Cancel on the CancellationToken.
                    // Exc.Message will be "A task was canceled." (in English).
                    // task.IsCanceled will be true.
                    if (task.IsCanceled)
                    {
                        throw new TaskCanceledException(task);
                    }
                    // I don't think this can occur, but if some other exception, rethrow it.
                    throw;
                }
                if (caughtException != null || ffmpegProcess.ExitCode != 0)
                {
                    OnException(messages, parameters, ffmpegProcess.ExitCode, caughtException);
                }
                else
                {
                    OnConversionCompleted(new ConversionCompleteEventArgs(parameters.InputFile, parameters.OutputFile));
                }
            }
        }

        private void OnException(List<string> messages, FFmpegParameters parameters, int exitCode, Exception caughtException)
        {
            var exceptionMessage = GetExceptionMessage(messages);
            var exception = new FFmpegException(exceptionMessage, caughtException, exitCode);
            OnConversionError(new ConversionErrorEventArgs(exception, parameters.InputFile, parameters.OutputFile));
        }

        private string GetExceptionMessage(List<string> messages)
            => messages.Count > 1
                ? messages[1] + messages[0]
                : string.Join(string.Empty, messages);

        private void FFmpegProcessOnErrorDataReceived(DataReceivedEventArgs e, FFmpegParameters parameters, ref Exception exception, List<string> messages)
        {
            var totalMediaDuration = new TimeSpan();
            if (e.Data == null)
                return;

            try
            {
                messages.Insert(0, e.Data);
                if (parameters.InputFile != null)
                {
                    RegexEngine.TestVideo(e.Data, parameters);
                    RegexEngine.TestAudio(e.Data, parameters);

                    var matchDuration = RegexEngine._index[RegexEngine.Find.Duration].Match(e.Data);
                    if (matchDuration.Success)
                    {
                        if (parameters.InputFile.MetaData == null)
                            parameters.InputFile.MetaData = new MetaData { FileInfo = parameters.InputFile.FileInfo };

                        TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                        parameters.InputFile.MetaData.Duration = totalMediaDuration;
                    }
                }

                if (RegexEngine.IsProgressData(e.Data, out var progressData))
                {
                    progressData.TotalDuration = totalMediaDuration;
                    OnProgressChanged(new ConversionProgressEventArgs(progressData, parameters.InputFile, parameters.OutputFile));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        private ProcessStartInfo GenerateStartInfo(string ffmpegPath, string arguments) => new ProcessStartInfo
        {
            // -y overwrite output files
            Arguments = "-y " + arguments,
            FileName = ffmpegPath,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        public event Action<ConversionProgressEventArgs> Progress;
        public event Action<ConversionCompleteEventArgs> Completed;
        public event Action<ConversionErrorEventArgs> Error;
        public event Action<ConversionDataEventArgs> Data;

        private void OnProgressChanged(ConversionProgressEventArgs eventArgs) => Progress?.Invoke(eventArgs);

        private void OnConversionCompleted(ConversionCompleteEventArgs eventArgs) => Completed?.Invoke(eventArgs);

        private void OnConversionError(ConversionErrorEventArgs eventArgs) => Error?.Invoke(eventArgs);

        private void OnData(ConversionDataEventArgs eventArgs) => Data?.Invoke(eventArgs);
    }
}
