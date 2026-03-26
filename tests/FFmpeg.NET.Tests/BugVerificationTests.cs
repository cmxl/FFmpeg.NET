using FFmpeg.NET.Enums;
using FFmpeg.NET.Exceptions;
using FFmpeg.NET.Tests.Fixtures;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FFmpeg.NET.Tests
{
    public class BugVerificationTests : IClassFixture<MediaFileFixture>
    {
        private readonly MediaFileFixture _fixture;
        private readonly ITestOutputHelper _output;

        public BugVerificationTests(MediaFileFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        /// <summary>
        /// Verifies fix for: Engine.ConvertAsync(IInputArgument, Stream, ConversionOptions, CancellationToken)
        /// previously deadlocked because pipe.WaitForConnectionAsync was awaited before
        /// process.ExecuteAsync started the FFmpeg process.
        ///
        /// Fix: Start process before waiting for pipe connection, use try/finally for cleanup.
        /// Location: Engine.cs
        /// </summary>
        [Fact]
        public async Task Pipe_ConvertAsync_Produces_Output()
        {
            var engine = new Engine(_fixture.FFmpegPath);
            var options = new ConversionOptions { VideoFormat = VideoFormat.mpegts };
            var outputStream = new MemoryStream();

            bool dataEventFired = false;
            engine.Data += (s, e) => dataEventFired = true;

            await engine.ConvertAsync(_fixture.VideoFile, outputStream, options, CancellationToken.None);

            _output.WriteLine($"Output stream length: {outputStream.Length} bytes");

            Assert.True(outputStream.Length > 0, "Pipe conversion should produce output data");
            Assert.True(dataEventFired, "Engine Data events should fire during conversion");
        }

        /// <summary>
        /// Verifies fix for: Engine.ConvertAsync(IInputArgument, ConversionOptions, CancellationToken)
        /// previously deadlocked because it delegates to the pipe-based overload above.
        ///
        /// Fix: Same as above (delegates to the fixed overload).
        /// Location: Engine.cs
        /// </summary>
        [Fact]
        public async Task Stream_Return_ConvertAsync_Produces_Output()
        {
            var engine = new Engine(_fixture.FFmpegPath);
            var options = new ConversionOptions { VideoFormat = VideoFormat.mpegts };

            var result = await engine.ConvertAsync((IInputArgument)_fixture.VideoFile, options, CancellationToken.None);

            _output.WriteLine($"Returned stream length: {result.Length} bytes");

            Assert.True(result.Length > 0, "Stream return conversion should produce output data");
        }

        /// <summary>
        /// Verifies fix for: FFmpegProcess.GetExceptionMessage concatenated the last two
        /// stderr lines without any separator, producing garbled messages like:
        ///   "[context]Error message"
        ///
        /// Fix: Added Environment.NewLine between the two lines.
        /// Location: FFmpegProcess.cs - GetExceptionMessage
        /// </summary>
        [Fact]
        public async Task Error_Message_Contains_Line_Separator()
        {
            var engine = new Engine(_fixture.FFmpegPath);
            var output = new OutputFile("test.txt"); // .txt is not a valid media format
            var input = _fixture.VideoFile;

            FFmpegException capturedException = null;
            engine.Error += (s, e) => capturedException = e.Exception;

            await engine.ConvertAsync(input, output, default);

            Assert.NotNull(capturedException);
            Assert.NotEmpty(capturedException.Message);

            _output.WriteLine($"Error message: [{capturedException.Message}]");

            // The two stderr lines should now be separated by a newline
            Assert.Contains(Environment.NewLine, capturedException.Message);
        }

        /// <summary>
        /// Verifies fix for: ConvertAsync(IArgument, Stream, CancellationToken) previously
        /// used ContinueWith(async ...) which returned Task&lt;Task&gt;. Task.WhenAll only
        /// awaited the outer task, making CopyToAsync fire-and-forget. The pipe could be
        /// disposed while data was still being read.
        ///
        /// Fix: Replaced Task.WhenAll/ContinueWith with sequential await and try/finally.
        /// Location: Engine.cs
        /// </summary>
        [Fact]
        public async Task IArgument_ConvertAsync_Produces_Complete_Output()
        {
            var engine = new Engine(_fixture.FFmpegPath);
            var outputStream = new MemoryStream();

            var inputPath = _fixture.VideoFile.FileInfo.FullName;
            var argument = new TestArgument($"-i \"{inputPath}\" -f mpegts");

            await engine.ConvertAsync(argument, outputStream, CancellationToken.None);

            _output.WriteLine($"Output stream length: {outputStream.Length} bytes");

            Assert.True(outputStream.Length > 0, "IArgument pipe conversion should produce output data");
        }

        private class TestArgument : IArgument
        {
            public string Argument { get; }
            public TestArgument(string arg) => Argument = arg;
        }
    }
}
