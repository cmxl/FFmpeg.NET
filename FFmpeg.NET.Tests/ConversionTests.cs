using System;
using System.IO;
using FFmpeg.NET.Events;
using FFmpeg.NET.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace FFmpeg.NET.Tests
{
    public class ConversionTests : IClassFixture<MediaFileFixture>
    {
        public ConversionTests(MediaFileFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        private readonly MediaFileFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;

        [Fact]
        public void FFmpeg_Invokes_ConversionCompleteEvent()
        {
            var output = new MediaFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"MediaFiles\conversionTest.mp4")));

            ConversionCompleteEventArgs completeEventArgs = null;

            var ffmpeg = new Engine.FFmpeg();
            ffmpeg.Complete += (sender, args) => { 
                completeEventArgs = args;
                _outputHelper.WriteLine("ConversionCompletedEvent: {0}", args);
            };
            ffmpeg.Convert(_fixture.VideoFile, output);
            
            Assert.True(File.Exists(output.FileInfo.FullName));
            output.FileInfo.Delete();
            Assert.False(File.Exists(output.FileInfo.FullName));

            Assert.NotNull(completeEventArgs);
            Assert.NotNull(completeEventArgs.Output);
            Assert.Equal(output, completeEventArgs.Output);
        }

        [Fact]
        public void FFmpeg_Should_Throw_Exception_On_Invalid_OutputFile()
        {
            var ffmpeg = new Engine.FFmpeg();
            var output = new MediaFile("test.txt");
            var input = _fixture.VideoFile;

            var e = Assert.Raises<ConversionErrorEventArgs>(
                x => ffmpeg.Error += x,
                x => ffmpeg.Error -= x,
                () => ffmpeg.Convert(input, output));

            Assert.NotNull(e);
            Assert.Equal(e.Sender, ffmpeg);
            Assert.Equal(input, e.Arguments.Input);
            Assert.Equal(output, e.Arguments.Output);
            Assert.Equal(1, e.Arguments.Exception.ExitCode);
        }
    }
}