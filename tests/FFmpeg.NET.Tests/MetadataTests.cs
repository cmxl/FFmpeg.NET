using System;
using System.IO;
using FFmpeg.NET.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FFmpeg.NET.Tests
{
    public class MetaDataTests : IClassFixture<MediaFileFixture>
    {
        public MetaDataTests(MediaFileFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        private readonly MediaFileFixture _fixture;
        private readonly ITestOutputHelper _output;

        // Create a 31 hour audio file.
        // Use ffmpeg.exe to copy the audio from SampleAudio_0.4mb.mp3 enough times so that the 
        // result is > 24 hours in duration. 
        internal static async Task CreateLongAudioFile(Engine ffmpeg, MediaFile inputFile)
        {
            FileInfo outputFileInfo = new FileInfo("LongAudio.mp3");
            if (outputFileInfo.Exists)
            {
                return;
            }

            MediaFile output = new MediaFile(outputFileInfo);
            MediaFile temp1 = new MediaFile("Long1.mp3");
            MediaFile temp2 = new MediaFile("Long2.mp3");
            MediaFile temp3 = new MediaFile("Long3.mp3");
            MediaFile temp4 = new MediaFile("Long4.mp3");

            string p = $"-i \"{inputFile.FileInfo.FullName}\" -ar 8K -q:a 9 -ac 1 -f mp3 -y \"{temp1.FileInfo.FullName}\"";

            await ffmpeg.ExecuteAsync(p, default).ConfigureAwait(false);

            string fn = $"{temp1.FileInfo.Name}";
            p = $"-i \"concat:{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}\" -c copy -y \"{temp2.FileInfo.Name}\"";
            await ffmpeg.ExecuteAsync(p, default).ConfigureAwait(false);

            fn = $"{temp2.FileInfo.Name}";
            p = $"-i \"concat:{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}\" -c copy -y \"{temp3.FileInfo.Name}\"";
            await ffmpeg.ExecuteAsync(p, default).ConfigureAwait(false);

            fn = $"{temp3.FileInfo.Name}";
            p = $"-i \"concat:{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}|{fn}\" -c copy -y \"{temp4.FileInfo.Name}\"";
            await ffmpeg.ExecuteAsync(p, default).ConfigureAwait(false);

            fn = $"{temp4.FileInfo.Name}";
            p = $"-i \"concat:{fn}|{fn}|{fn}|{fn}\" -c copy -y \"{output.FileInfo.Name}\"";
            await ffmpeg.ExecuteAsync(p, default).ConfigureAwait(false);

            temp1.FileInfo.Delete();
            temp2.FileInfo.Delete();
            temp3.FileInfo.Delete();
            temp4.FileInfo.Delete();
        }

        [Fact]
        public async Task FFmpeg_Can_Read_Audio_Metadata()
        {
            var ffmpeg = new Engine(_fixture.FFmpegPath);

            var audioFile = _fixture.AudioFile;
            var metaData = await ffmpeg.GetMetaDataAsync(audioFile).ConfigureAwait(false);

            Assert.NotNull(metaData);

            Assert.Equal(metaData.FileInfo, audioFile.FileInfo);

            Assert.NotNull(metaData.AudioData);
            Assert.Equal("mp3", metaData.AudioData.Format);
            Assert.Equal("44100 Hz", metaData.AudioData.SampleRate);
            Assert.Equal("stereo", metaData.AudioData.ChannelOutput);
            Assert.Equal(128, metaData.AudioData.BitRateKbs);
            Assert.Null(metaData.VideoData);
        }

        [Fact]
        public async Task FFmpeg_Can_Read_Video_Metadata()
        {
            var ffmpeg = new Engine(_fixture.FFmpegPath);
            ffmpeg.Data += (s, e) => _output.WriteLine(e.Data);

            var videoFile = _fixture.VideoFile;
            var metaData = await ffmpeg.GetMetaDataAsync(videoFile).ConfigureAwait(false);

            Assert.NotNull(metaData);
            Assert.Equal(metaData.FileInfo, videoFile.FileInfo);
            Assert.NotNull(metaData.VideoData);
            Assert.Equal("h264 (Main) (avc1 / 0x31637661)", metaData.VideoData.Format);
            Assert.Equal("yuv420p", metaData.VideoData.ColorModel);
            Assert.Equal("1280x720", metaData.VideoData.FrameSize);
            Assert.Equal(1205, metaData.VideoData.BitRateKbs);
            Assert.Equal(25, metaData.VideoData.Fps);

            Assert.NotNull(metaData.AudioData);
            Assert.Equal("aac (LC) (mp4a / 0x6134706D)", metaData.AudioData.Format);
            Assert.Equal("48000 Hz", metaData.AudioData.SampleRate);
            Assert.Equal("5.1", metaData.AudioData.ChannelOutput);
            Assert.Equal(384, metaData.AudioData.BitRateKbs);

            Assert.Equal(metaData.Duration, new TimeSpan(0, 0, 0, 5, 310));
        }

        [Theory]
        [InlineData("Stream #0:0(und): Video: h264 (Main) (avc1 / 0x31637661), yuv420p, 1280x720 [SAR 1:1 DAR 16:9], 1205 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)", 1205, "yuv420p", "1280x720", "h264 (Main) (avc1 / 0x31637661)")]
        [InlineData("Stream #0:0(und): Video: hevc (Main 10) (hvc1 / 0x31637668), yuv420p10le(tv, bt2020/arib-std-b67, progressive), 1920x1080, 8517 kb/s, 29.98 fps, 29.97 tbr, 600 tbn, 600 tbc (default)", 8517, "yuv420p10le(tv, bt2020/arib-std-b67, progressive)", "1920x1080" , "hevc (Main 10) (hvc1 / 0x31637668)")]
        public void RegexEngine_Can_Read_Video_MetaData(string data, int bitrate, string colorModel, string frameSize, string format)
        {
            var param = new FFmpegParameters
            {
                Input = new InputFile(_fixture.VideoFile.FileInfo)
            };
            RegexEngine.TestVideo(data, param);

            Assert.NotNull(param.Input.MetaData);
            Assert.NotNull(param.Input.MetaData.VideoData);
            
            Assert.Equal(bitrate, param.Input.MetaData.VideoData.BitRateKbs);
            Assert.Equal(colorModel, param.Input.MetaData.VideoData.ColorModel);
            Assert.Equal(frameSize, param.Input.MetaData.VideoData.FrameSize);
            Assert.Equal(format, param.Input.MetaData.VideoData.Format);
        }

        [Fact]
        public async Task CustomParameters()
        {
            var ffmpeg = new Engine(_fixture.FFmpegPath);
            await ffmpeg.ExecuteAsync($"-i \"{_fixture.VideoFile.FileInfo.FullName}\" -f ffmetadata -", default).ConfigureAwait(false);
        }

        [Fact]
        public async Task FFmpeg_Can_Read_Long_Duration()
        {
            Engine ffmpeg = new Engine(_fixture.FFmpegPath);

            await CreateLongAudioFile(ffmpeg, _fixture.AudioFile).ConfigureAwait(false);

            FileInfo audioFileInfo = new FileInfo("LongAudio.mp3");
            InputFile audioFile = new InputFile(audioFileInfo);

            MetaData metaData = await ffmpeg.GetMetaDataAsync(audioFile).ConfigureAwait(false);

            Assert.NotNull(metaData);
            Assert.NotNull(metaData.AudioData);
            Assert.InRange(metaData.Duration.TotalHours, 30.0, 40.0);
        }
    }
}