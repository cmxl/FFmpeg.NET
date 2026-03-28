using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace FFmpeg.NET.CompatTests
{
    public class StandardInputWriterTests
    {
        [Fact]
        public void IsOpen_Returns_False_When_No_Process()
        {
            var writer = new StandardInputWriter();
            Assert.False(writer.IsOpen);
        }

        [Fact]
        public async Task WriteAsync_ByteArray_Does_Not_Throw_When_No_Process()
        {
            var writer = new StandardInputWriter();
            var data = new byte[] { 1, 2, 3 };
            await writer.WriteAsync(data, 0, data.Length);
        }

        [Fact]
        public void Close_Does_Not_Throw_When_No_Process()
        {
            var writer = new StandardInputWriter();
            writer.Close();
        }

        [Fact]
        public void WriteAsync_ByteArray_Overload_Exists_On_Both_Targets()
        {
            var method = typeof(StandardInputWriter).GetMethod(
                "WriteAsync",
                new[] { typeof(byte[]), typeof(int), typeof(int), typeof(System.Threading.CancellationToken) });

            Assert.NotNull(method);
        }

#if NET5_0_OR_GREATER
        [Fact]
        public void WriteAsync_ReadOnlyMemory_Overload_Exists_On_NetStandard21()
        {
            var method = typeof(StandardInputWriter).GetMethod(
                "WriteAsync",
                new[] { typeof(ReadOnlyMemory<byte>), typeof(System.Threading.CancellationToken) });

            Assert.NotNull(method);
        }

        [Fact]
        public async Task WriteAsync_ReadOnlyMemory_Does_Not_Throw_When_No_Process()
        {
            var writer = new StandardInputWriter();
            var data = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 });
            await writer.WriteAsync(data);
        }
#endif

#if NETFRAMEWORK
        [Fact]
        public void WriteAsync_ReadOnlyMemory_Overload_Does_Not_Exist_On_NetStandard20()
        {
            var methods = typeof(StandardInputWriter).GetMethods()
                .Where(m => m.Name == "WriteAsync")
                .ToArray();

            // Only the byte[] overload should exist
            Assert.Single(methods);
        }
#endif

        [Fact]
        public async Task HandleProcessExitedAsync_Clears_Process()
        {
            var writer = new StandardInputWriter();

            await writer.HandleProcessExitedAsync(null, default);

            Assert.False(writer.IsOpen);
        }
    }
}
