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
        FFmpegParameters parameters;
        readonly string ffmpegFilePath;
        readonly private CancellationToken cancellationToken;

        List<string> messages;
        Exception caughtException = null;

        public FFmpegProcess(FFmpegParameters parameters, string ffmpegFilePath, CancellationToken cancellationToken = default)
        {
            this.parameters = parameters;
            this.ffmpegFilePath = ffmpegFilePath;
            this.cancellationToken = cancellationToken;
        }

        public async Task ExecuteAsync()
        {
            messages = new List<string>();
            caughtException = null;
            string arguments = FFmpegArgumentBuilder.Build(parameters);
            ProcessStartInfo startInfo = GenerateStartInfo(ffmpegFilePath, arguments);
            await ExecuteAsync(startInfo, parameters, cancellationToken);
        }

        private async Task ExecuteAsync(ProcessStartInfo startInfo, FFmpegParameters parameters, CancellationToken cancellationToken = default)
        {
            using (Process ffmpegProcess = new Process() { StartInfo = startInfo })
            {
                ffmpegProcess.ErrorDataReceived += OnDataHandler;
                

                Task<int> task = null;
                try
                {
                    task = ffmpegProcess.WaitForExitAsync(null, cancellationToken);
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
                finally
                {
                    task?.Dispose();
                    ffmpegProcess.ErrorDataReceived -= OnDataHandler;
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

        private void OnDataHandler(object sender, DataReceivedEventArgs e)
        {
            OnData(new ConversionDataEventArgs(e.Data, parameters.InputFile, parameters.OutputFile));
            FFmpegProcessOnErrorDataReceived(e, parameters, ref caughtException, messages);
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

                        RegexEngine.TimeSpanLargeTryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                        parameters.InputFile.MetaData.Duration = totalMediaDuration;
                    }
                }

                if (RegexEngine.IsProgressData(e.Data, out var progressData))
                {
                    if (parameters.InputFile != null)
                    {
                        progressData.TotalDuration = parameters.InputFile.MetaData?.Duration ?? totalMediaDuration;
                    }

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
