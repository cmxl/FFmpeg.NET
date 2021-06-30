using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg.NET.Models
{
    public class MediaInfo
    {
        public MediaInfo( double bitrate, TimeSpan totalDuration)
        {
    
            Bitrate = bitrate;
            TotalDuration = totalDuration;
        }

        public double Bitrate { get; }
        public TimeSpan TotalDuration { get; internal set; }

    }
}
