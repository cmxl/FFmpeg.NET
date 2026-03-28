using System.IO;
using Xunit;

namespace FFmpeg.NET.CompatTests
{
    public class PropertyTests
    {
        [Fact]
        public void InputFile_UseStandardInput_Returns_False()
        {
            var file = new InputFile(new FileInfo("test.mp4"));
            Assert.False(file.UseStandardInput);
        }

        [Fact]
        public void InputFile_Name_Returns_FullName()
        {
            var fileInfo = new FileInfo("test.mp4");
            var file = new InputFile(fileInfo);
            Assert.Equal(fileInfo.FullName, file.Name);
        }

        [Fact]
        public void InputFile_Argument_Returns_Quoted_FullName()
        {
            var fileInfo = new FileInfo("test.mp4");
            var file = new InputFile(fileInfo);
            Assert.Equal($"\"{fileInfo.FullName}\"", file.Argument);
        }

        [Fact]
        public void OutputFile_Name_Returns_FullName()
        {
            var fileInfo = new FileInfo("output.mp4");
            var file = new OutputFile(fileInfo);
            Assert.Equal(fileInfo.FullName, file.Name);
        }

        [Fact]
        public void OutputFile_Argument_Returns_Quoted_FullName()
        {
            var fileInfo = new FileInfo("output.mp4");
            var file = new OutputFile(fileInfo);
            Assert.Equal($"\"{fileInfo.FullName}\"", file.Argument);
        }

        [Fact]
        public void InputPipe_UseStandardInput_Returns_False()
        {
            var pipe = new InputPipe(@"\\.\pipe\test");
            Assert.False(pipe.UseStandardInput);
        }

        [Fact]
        public void InputPipe_Name_Returns_PipePath()
        {
            var pipePath = @"\\.\pipe\test";
            var pipe = new InputPipe(pipePath);
            Assert.Equal(pipePath, pipe.Name);
        }

        [Fact]
        public void InputPipe_Argument_Returns_Quoted_PipePath()
        {
            var pipePath = @"\\.\pipe\test";
            var pipe = new InputPipe(pipePath);
            Assert.Equal($"\"{pipePath}\"", pipe.Argument);
        }

        [Fact]
        public void OutputPipe_Name_Returns_PipePath()
        {
            var pipePath = @"\\.\pipe\test";
            var pipe = new OutputPipe(pipePath);
            Assert.Equal(pipePath, pipe.Name);
        }

        [Fact]
        public void OutputPipe_Argument_Returns_Quoted_PipePath()
        {
            var pipePath = @"\\.\pipe\test";
            var pipe = new OutputPipe(pipePath);
            Assert.Equal($"\"{pipePath}\"", pipe.Argument);
        }

        [Fact]
        public void StreamInput_UseStandardInput_Returns_True()
        {
            using var input = new StreamInput(new MemoryStream());
            Assert.True(input.UseStandardInput);
        }

        [Fact]
        public void StreamInput_Argument_Returns_Dash()
        {
            using var input = new StreamInput(new MemoryStream());
            Assert.Equal("-", input.Argument);
        }

        [Fact]
        public void StreamInput_Name_Returns_Stream()
        {
            using var input = new StreamInput(new MemoryStream());
            Assert.Equal("stream", input.Name);
        }

        [Fact]
        public void StandardInputWriter_UseStandardInput_Returns_True()
        {
            var writer = new StandardInputWriter();
            Assert.True(writer.UseStandardInput);
        }

        [Fact]
        public void StandardInputWriter_Argument_Returns_Dash()
        {
            var writer = new StandardInputWriter();
            Assert.Equal("-", writer.Argument);
        }

        [Fact]
        public void StandardInputWriter_Name_Returns_Stdin()
        {
            var writer = new StandardInputWriter();
            Assert.Equal("stdin", writer.Name);
        }
    }
}
