using System;
using System.IO;

namespace FFmpeg.NET.Tests.Fixtures
{
    public class MediaFileFixture
    {
        public MediaFile VideoFile => new MediaFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleVideo_1280x720_1mb.mp4")));
        public MediaFile AudioFile => new MediaFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleAudio_0.4mb.mp3")));
    }
}