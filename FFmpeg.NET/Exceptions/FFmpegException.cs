using System;

namespace FFmpeg.NET.Exceptions
{
    public class FFmpegException : Exception
    {
        public FFmpegException(int exitCode)
        {
            ExitCode = exitCode;
        }

        public FFmpegException(string message, int exitCode) : base(message)
        {
            ExitCode = exitCode;
        }

        public FFmpegException(string message, Exception innerException, int exitCode) : base(message, innerException)
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}