using FFmpeg.NET.Models;
using System;

namespace FFmpeg.NET.Events
{
    public class ConversionProgressEventArgs : EventArgs
    {
        internal ConversionProgressEventArgs(ProgressData progressData, IInputArgument input, IOutputArgument output)
        {
            Input = input;
            Output = output;
            TotalDuration = progressData.TotalDuration;
            ProcessedDuration = progressData.ProcessedDuration;
            Frame = progressData.Frame;
            Fps = progressData.Fps;
            SizeKb = progressData.SizeKb;
            Bitrate = progressData.Bitrate;
        }

        public long? Frame { get; }
        public double? Fps { get; }
        public int? SizeKb { get; }
        public TimeSpan ProcessedDuration { get; }
        public double? Bitrate { get; }
        public TimeSpan TotalDuration { get; }
        public IOutputArgument Output { get; }
        public IInputArgument Input { get; }

        public override string ToString()
            => $"[{Input?.Name} => {Output?.Name}]\nFrame: {Frame}\nFps: {Fps}\nSize: {SizeKb}kb\nProcessedDuration: {ProcessedDuration}\nBitrate: {Bitrate}\nTotalDuration: {TotalDuration}";
    }
}