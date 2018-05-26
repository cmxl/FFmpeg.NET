using System;
using FFmpeg.NET.Exceptions;

namespace FFmpeg.NET.Events
{
    public class ConversionErrorEventArgs : EventArgs
    {
        public ConversionErrorEventArgs(FFmpegException exception, MediaFile input, MediaFile output)
        {
            Exception = exception;
            Input = input;
            Output = output;
        }

        public FFmpegException Exception { get; }
        public MediaFile Input { get; }
        public MediaFile Output { get; }
    }
}