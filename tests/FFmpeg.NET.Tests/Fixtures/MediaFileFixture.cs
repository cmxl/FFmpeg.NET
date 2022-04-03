using System;
using System.IO;

namespace FFmpeg.NET.Tests.Fixtures
{
    public class MediaFileFixture : BaseFixture
    {
        public InputFile VideoFile => new InputFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleVideo_1280x720_1mb.mp4")));
        public InputFile FlvVideoFile => new InputFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleVideo_1280x720_1mb.flv")));
        public InputFile AudioFile => new InputFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaFiles\SampleAudio_0.4mb.mp3")));
    }
}