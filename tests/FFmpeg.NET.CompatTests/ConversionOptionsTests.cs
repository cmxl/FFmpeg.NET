using System;
using FFmpeg.NET.Enums;
using Xunit;

namespace FFmpeg.NET.CompatTests
{
    public class ConversionOptionsTests
    {
        [Fact]
        public void Default_ConversionOptions_Has_Default_Enum_Values()
        {
            var options = new ConversionOptions();

            Assert.Equal(AudioSampleRate.Default, options.AudioSampleRate);
            Assert.Equal(VideoSize.Default, options.VideoSize);
            Assert.Equal(HWAccel.None, options.HWAccel);
        }

        [Fact]
        public void Can_Set_Audio_Properties()
        {
            var options = new ConversionOptions
            {
                AudioBitRate = 128,
                AudioSampleRate = AudioSampleRate.Hz22050
            };

            Assert.Equal(128, options.AudioBitRate);
            Assert.Equal(AudioSampleRate.Hz22050, options.AudioSampleRate);
        }

        [Fact]
        public void Can_Set_Video_Properties()
        {
            var options = new ConversionOptions
            {
                VideoFps = 30,
                VideoSize = VideoSize.Hd720
            };

            Assert.Equal(30, options.VideoFps);
            Assert.Equal(VideoSize.Hd720, options.VideoSize);
        }

        [Fact]
        public void Can_Set_Seek_And_MaxDuration()
        {
            var options = new ConversionOptions
            {
                Seek = TimeSpan.FromSeconds(10),
                MaxVideoDuration = TimeSpan.FromMinutes(5)
            };

            Assert.Equal(TimeSpan.FromSeconds(10), options.Seek);
            Assert.Equal(TimeSpan.FromMinutes(5), options.MaxVideoDuration);
        }

        [Fact]
        public void Can_Set_ExtraArguments()
        {
            var options = new ConversionOptions
            {
                ExtraArguments = "-vf scale=1280:720"
            };

            Assert.Equal("-vf scale=1280:720", options.ExtraArguments);
        }

        [Fact]
        public void Can_Set_HardwareAcceleration()
        {
            var options = new ConversionOptions
            {
                HWAccel = HWAccel.cuda
            };

            Assert.Equal(HWAccel.cuda, options.HWAccel);
        }
    }
}
