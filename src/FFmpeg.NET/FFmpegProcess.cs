using FFmpeg.NET.Events;
using FFmpeg.NET.Exceptions;
using FFmpeg.NET.Extensions;
using FFmpeg.NET.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpeg.NET
{
    internal sealed class FFmpegProcess
    {
        private readonly FFmpegParameters _parameters;
        private readonly string _ffmpegFilePath;

        private MediaInfo _mediaInfo;
        private List<string> _messages;
        private Exception _caughtException = null;

        public FFmpegProcess(FFmpegParameters parameters, string ffmpegFilePath)
        {
            _parameters = parameters;
            _ffmpegFilePath = ffmpegFilePath;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _messages = new List<string>();
            _caughtException = null;
            string arguments = FFmpegArgumentBuilder.Build(_parameters);
            ProcessStartInfo startInfo = GenerateStartInfo(_ffmpegFilePath, arguments);
            await ExecuteAsync(startInfo, _parameters, cancellationToken).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(ProcessStartInfo startInfo, FFmpegParameters parameters, CancellationToken cancellationToken)
        {
            using var ffmpegProcess = new Process() { StartInfo = startInfo };
            ffmpegProcess.ErrorDataReceived += OnDataHandler;

            Task<int> task = null;
            try
            {
                var useStandardInput = _parameters.Input?.UseStandardInput == true;
                task = ffmpegProcess.WaitForExitAsync(useStandardInput, null, cancellationToken);
                
                var inputHandler = _parameters.Input as IProcessExecutionHandler;

                if (inputHandler != null)
                {
                    await inputHandler.HandleProcessStartedAsync(ffmpegProcess, cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    await task.ConfigureAwait(false);
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

                if (inputHandler != null)
                {
                    await inputHandler.HandleProcessExitedAsync(ffmpegProcess, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                task?.Dispose();
                ffmpegProcess.ErrorDataReceived -= OnDataHandler;
            }


            if (_caughtException != null || ffmpegProcess.ExitCode != 0)
            {
                OnException(_messages, parameters, ffmpegProcess.ExitCode, _caughtException);
            }
            else
            {
                OnConversionCompleted(new ConversionCompleteEventArgs(parameters.Input, parameters.Output));
            }
        }

        private void OnDataHandler(object sender, DataReceivedEventArgs e)
        {
            OnData(new ConversionDataEventArgs(e.Data, _parameters.Input, _parameters.Output));
            FFmpegProcessOnErrorDataReceived(e, _parameters, ref _caughtException, _messages);
        }
        private void TryUpdateMediaInfo(string data)
        {
            if (_mediaInfo == null)
                if (RegexEngine.IsMediaInfo(data, out var newMediaInfo))
                {
                    _mediaInfo = newMediaInfo;
                }
        }
        private void OnException(List<string> messages, FFmpegParameters parameters, int exitCode, Exception caughtException)
        {
            var exceptionMessage = GetExceptionMessage(messages);
            var exception = new FFmpegException(exceptionMessage, caughtException, exitCode);
            OnConversionError(new ConversionErrorEventArgs(exception, parameters.Input, parameters.Output));
        }

        private static string GetExceptionMessage(List<string> messages)
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
                TryUpdateMediaInfo(e.Data);
                messages.Insert(0, e.Data);
                if (parameters.Input != null)
                {
                    RegexEngine.TestVideo(e.Data, parameters);
                    RegexEngine.TestAudio(e.Data, parameters);

                    var matchDuration = RegexEngine._index[RegexEngine.Find.Duration].Match(e.Data);
                    if (matchDuration.Success)
                    {
                        if (parameters.Input is IHasMetaData input)
                        {
                            if (input.MetaData == null)
                                input.MetaData = new MetaData
                                {
                                    FileInfo = (parameters.Input as MediaFile)?.FileInfo
                                };

                            RegexEngine.TimeSpanLargeTryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                            input.MetaData.Duration = totalMediaDuration;
                        }
                    }
                }

                if (RegexEngine.IsProgressData(e.Data, out var progressData))
                {
                    if (parameters.Input != null)
                    {
                        progressData.TotalDuration = parameters.Input.MetaData?.Duration ?? totalMediaDuration;
                    }

                    OnProgressChanged(new ConversionProgressEventArgs(progressData, parameters.Input, parameters.Output, _mediaInfo));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        private static ProcessStartInfo GenerateStartInfo(string ffmpegPath, string arguments) => new ProcessStartInfo()
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
