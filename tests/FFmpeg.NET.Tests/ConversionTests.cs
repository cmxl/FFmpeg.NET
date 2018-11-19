using System;
using System.IO;
using System.Threading.Tasks;
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
        public async Task FFmpeg_Invokes_ConversionCompleteEvent()
        {
            var output = new MediaFile(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"MediaFiles\conversionTest.mp4")));
            var ffmpeg = new Engine.FFmpeg(_fixture.FFmpegPath);

            var e = await Assert.RaisesAsync<ConversionCompleteEventArgs>(
                x => ffmpeg.Complete += x,
                x => ffmpeg.Complete -= x,
                async () => await ffmpeg.ConvertAsync(_fixture.VideoFile, output)
            );
            
            Assert.True(File.Exists(output.FileInfo.FullName));
            output.FileInfo.Delete();
            Assert.False(File.Exists(output.FileInfo.FullName));

            Assert.NotNull(e);
            Assert.Equal(e.Sender, ffmpeg);
            Assert.Equal(_fixture.VideoFile.FileInfo.FullName, e.Arguments.Input.FileInfo.FullName);
            Assert.Equal(output.FileInfo.FullName, e.Arguments.Output.FileInfo.FullName);
        }

        [Fact]
        public async Task FFmpeg_Should_Throw_Exception_On_Invalid_OutputFile()
        {
            var ffmpeg = new Engine.FFmpeg(_fixture.FFmpegPath);
            var output = new MediaFile("test.txt");
            var input = _fixture.VideoFile;

            var e = await Assert.RaisesAsync<ConversionErrorEventArgs>(
                x => ffmpeg.Error += x,
                x => ffmpeg.Error -= x,
                async () => await ffmpeg.ConvertAsync(input, output));

            Assert.NotNull(e);
            Assert.Equal(e.Sender, ffmpeg);
            Assert.Equal(input, e.Arguments.Input);
            Assert.Equal(output, e.Arguments.Output);
            Assert.Equal(1, e.Arguments.Exception.ExitCode);
        }
    }
}