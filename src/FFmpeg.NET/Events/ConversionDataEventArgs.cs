using System;

namespace FFmpeg.NET.Events
{
    public class ConversionDataEventArgs : EventArgs
    {
        public ConversionDataEventArgs(string data, IInputArgument input, IOutputArgument output)
        {
            Data = data;
            Input = input;
            Output = output;
        }

        public string Data { get; }
        public IInputArgument Input { get; }
        public IOutputArgument Output { get; }
    }
}