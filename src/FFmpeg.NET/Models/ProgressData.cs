using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.NET.Models
{
    internal class ProgressData
    {
        public ProgressData(TimeSpan processed, TimeSpan totalDuration, long? frame, double? fps, int? sizeKb,
            double? bitrate)
        {
            TotalDuration = totalDuration;
            ProcessedDuration = processed;
            Frame = frame;
            Fps = fps;
            SizeKb = sizeKb;
            Bitrate = bitrate;
        }

        public long? Frame { get; }
        public double? Fps { get; }
        public int? SizeKb { get; }
        public TimeSpan ProcessedDuration { get; }
        public double? Bitrate { get; }
        public TimeSpan TotalDuration { get; internal set; }

    }
}
