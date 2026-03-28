using System;
using System.Linq;
using Xunit;

namespace FFmpeg.NET.CompatTests
{
    public class InterfaceCompatTests
    {
        [Fact]
        public void StreamInput_Implements_IDisposable()
        {
            Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(StreamInput)));
        }

        [Fact]
        public void StreamInput_Implements_IInputArgument()
        {
            Assert.True(typeof(IInputArgument).IsAssignableFrom(typeof(StreamInput)));
        }

#if NET5_0_OR_GREATER
        [Fact]
        public void StreamInput_Implements_IAsyncDisposable_On_NetStandard21()
        {
            Assert.True(typeof(IAsyncDisposable).IsAssignableFrom(typeof(StreamInput)));
        }
#endif

#if NETFRAMEWORK
        [Fact]
        public void StreamInput_Does_Not_Implement_IAsyncDisposable_On_NetStandard20()
        {
            var interfaces = typeof(StreamInput).GetInterfaces();
            Assert.DoesNotContain(interfaces, i => i.Name == "IAsyncDisposable");
        }
#endif

        [Fact]
        public void InputFile_Implements_IInputArgument()
        {
            Assert.True(typeof(IInputArgument).IsAssignableFrom(typeof(InputFile)));
        }

        [Fact]
        public void InputPipe_Implements_IInputArgument()
        {
            Assert.True(typeof(IInputArgument).IsAssignableFrom(typeof(InputPipe)));
        }

        [Fact]
        public void OutputFile_Implements_IOutputArgument()
        {
            Assert.True(typeof(IOutputArgument).IsAssignableFrom(typeof(OutputFile)));
        }

        [Fact]
        public void OutputPipe_Implements_IOutputArgument()
        {
            Assert.True(typeof(IOutputArgument).IsAssignableFrom(typeof(OutputPipe)));
        }

        [Fact]
        public void StandardInputWriter_Implements_IInputArgument()
        {
            Assert.True(typeof(IInputArgument).IsAssignableFrom(typeof(StandardInputWriter)));
        }
    }
}
