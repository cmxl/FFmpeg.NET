using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET.Events;
using FFmpeg.NET.Exceptions;
using FFmpeg.NET.Extensions;

namespace FFmpeg.NET.Engine
{
    internal sealed class FFmpegProcess
    {
        public void Execute(FFmpegParameters parameters, string ffmpegFilePath)
        {
            ExecuteAsync(parameters, ffmpegFilePath).Wait();
        }

        public async Task ExecuteAsync(FFmpegParameters parameters, string ffmpegFilePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var argumentBuilder = new FFmpegArgumentBuilder();
            var arguments = argumentBuilder.Build(parameters);
            var startInfo = GenerateStartInfo(ffmpegFilePath, arguments);
            await ExecuteAsync(startInfo, parameters, cancellationToken);
        }


        private void Execute(ProcessStartInfo startInfo, FFmpegParameters parameters)
        {
            ExecuteAsync(startInfo, parameters).Wait();
        }

        private async Task ExecuteAsync(ProcessStartInfo startInfo, FFmpegParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var messages = new List<string>();
            Exception caughtException = null;

            using (var ffmpegProcess = Process.Start(startInfo))
            {
                if (ffmpegProcess == null)
                    throw new InvalidOperationException(Resources.Resources.Exceptions_FFmpeg_Process_Not_Running);

                ffmpegProcess.ErrorDataReceived += (sender, e) => OnData(new ConversionDataEventArgs(e.Data, parameters.InputFile, parameters.OutputFile));
                ffmpegProcess.ErrorDataReceived += (sender, e) => { FFmpegProcessOnErrorDataReceived(e, parameters, ref caughtException, messages); };

                ffmpegProcess.BeginErrorReadLine();
                await ffmpegProcess.WaitForExitAsync(cancellationToken);

                if (ffmpegProcess.ExitCode != 0 || caughtException != null)
                {
                    try
                    {
                        ffmpegProcess.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                        // swallow exceptions that are thrown when killing the process, 
                        // one possible candidate is the application ending naturally before we get a chance to kill it
                    }
                    catch (Win32Exception)
                    {
                    }

                    var exception = new FFmpegException(messages[1] + messages[0], caughtException, ffmpegProcess.ExitCode);
                    OnConversionError(new ConversionErrorEventArgs(exception, parameters.InputFile, parameters.OutputFile));
                }
                else
                {
                    OnConversionCompleted(new ConversionCompleteEventArgs(parameters.InputFile, parameters.OutputFile));
                }
            }
        }

        private void FFmpegProcessOnErrorDataReceived(DataReceivedEventArgs e, FFmpegParameters parameters, ref Exception exception, List<string> messages)
        {
            var totalMediaDuration = new TimeSpan();
            if (e.Data == null) return;

            try
            {
                messages.Insert(0, e.Data);
                if (parameters.InputFile != null)
                {
                    RegexEngine.TestVideo(e.Data, parameters);
                    RegexEngine.TestAudio(e.Data, parameters);

                    var matchDuration = RegexEngine.Index[RegexEngine.Find.Duration].Match(e.Data);
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

        private ProcessStartInfo GenerateStartInfo(string ffmpegPath, string arguments)
        {
            return new ProcessStartInfo
            {
                Arguments = "-nostdin -y -loglevel info " + arguments,
                FileName = ffmpegPath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        public event Action<ConversionProgressEventArgs> Progress;
        public event Action<ConversionCompleteEventArgs> Completed;
        public event Action<ConversionErrorEventArgs> Error;
        public event Action<ConversionDataEventArgs> Data;

        private void OnProgressChanged(ConversionProgressEventArgs eventArgs)
        {
            Progress?.Invoke(eventArgs);
        }

        private void OnConversionCompleted(ConversionCompleteEventArgs eventArgs)
        {
            Completed?.Invoke(eventArgs);
        }

        private void OnConversionError(ConversionErrorEventArgs eventArgs)
        {
            Error?.Invoke(eventArgs);
        }

        private void OnData(ConversionDataEventArgs eventArgs)
        {
            Data?.Invoke(eventArgs);
        }
    }
}