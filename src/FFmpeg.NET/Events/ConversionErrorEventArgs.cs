using FFmpeg.NET.Exceptions;
using System;

namespace FFmpeg.NET.Events
{
    public class ConversionErrorEventArgs : EventArgs
    {
        public ConversionErrorEventArgs(FFmpegException exception, IInputArgument input, IOutputArgument output)
        {
            Exception = exception;
            Input = input;
            Output = output;
        }

        public FFmpegException Exception { get; }
        public IInputArgument Input { get; }
        public IOutputArgument Output { get; }
    }
}