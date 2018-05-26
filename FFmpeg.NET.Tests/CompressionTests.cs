using System.IO;
using System.Text;
using FFmpeg.NET.Compression;
using FFmpeg.NET.Tests.Fixtures;
using Xunit;

namespace FFmpeg.NET.Tests
{
    public class CompressionTests : IClassFixture<StringFixture>
    {
        private readonly StringFixture _fixture;
        public CompressionTests(StringFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GzipCompressor_Does_Actually_Compress()
        {
            var compressor = new GzipCompressor();
            var source = Encoding.GetEncoding("utf-16").GetBytes(_fixture.TestString);

            byte[] gzipBytes = null;

            using (var input = new MemoryStream(source))
            using (var output = new MemoryStream())
            {
                compressor.Compress(input, output);

                var sourceBytes = input.ToArray();
                gzipBytes = output.ToArray();

                Assert.Equal(source, sourceBytes);
                Assert.True(sourceBytes.Length > gzipBytes.Length);
                Assert.NotEqual(sourceBytes, gzipBytes);

                var gzipString = Encoding.UTF8.GetString(gzipBytes);
                Assert.NotEqual(_fixture.TestString, gzipString);
            }

            using (var input = new MemoryStream(gzipBytes))
            using (var output = new MemoryStream())
            {
                compressor.Decompress(input, output);

                var gzippedBytes = input.ToArray();
                var sourceBytes = output.ToArray();

                Assert.Equal(gzipBytes, gzippedBytes);
                Assert.True(gzippedBytes.Length < sourceBytes.Length);
                Assert.NotEqual(gzippedBytes, sourceBytes);
                Assert.Equal(source, sourceBytes);

                var sourceString = Encoding.GetEncoding("utf-16").GetString(sourceBytes);
                Assert.Equal(_fixture.TestString, sourceString);
            }
        }
    }
}