using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FFmpeg.NET.CompatTests
{
    public class StreamInputTests
    {
        [Fact]
        public void Ctor_Throws_On_Null_Stream()
        {
            Assert.Throws<ArgumentNullException>(() => new StreamInput(null));
        }

        [Fact]
        public void Ctor_Throws_On_Unreadable_Stream()
        {
            using var ms = new MemoryStream();
            ms.Close();
            Assert.Throws<ArgumentException>(() => new StreamInput(ms));
        }

        [Fact]
        public void Dispose_With_DisposeStream_True_Disposes_Stream()
        {
            var stream = new MemoryStream();
            var input = new StreamInput(stream, disposeStream: true);

            input.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
        }

        [Fact]
        public void Dispose_With_DisposeStream_False_Does_Not_Dispose_Stream()
        {
            var stream = new MemoryStream();
            var input = new StreamInput(stream, disposeStream: false);

            input.Dispose();

            // Stream should still be usable
            Assert.Equal(-1, stream.ReadByte());
        }

#if NET5_0_OR_GREATER
        [Fact]
        public async Task DisposeAsync_With_DisposeStream_True_Disposes_Stream()
        {
            var stream = new MemoryStream();
            var input = new StreamInput(stream, disposeStream: true);

            await input.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
        }

        [Fact]
        public async Task DisposeAsync_With_DisposeStream_False_Does_Not_Dispose_Stream()
        {
            var stream = new MemoryStream();
            var input = new StreamInput(stream, disposeStream: false);

            await input.DisposeAsync();

            Assert.Equal(-1, stream.ReadByte());
        }
#endif

        [Fact]
        public async Task HandleProcessExitedAsync_Returns_CompletedTask()
        {
            using var input = new StreamInput(new MemoryStream());
            await input.HandleProcessExitedAsync(null, default);
        }

        [Fact]
        public async Task HandleProcessStartedAsync_Returns_When_Process_Is_Null()
        {
            using var input = new StreamInput(new MemoryStream());
            await input.HandleProcessStartedAsync(null, default);
        }
    }
}
