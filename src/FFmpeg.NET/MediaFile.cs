using System.IO;

namespace FFmpeg.NET
{
    public abstract class MediaFile : IHasMetaData
    {
        public MediaFile(string file) : this(new FileInfo(file))
        {
        }

        public MediaFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        public FileInfo FileInfo { get; }
        public MetaData MetaData { get; set; }
    }

    public class InputFile : MediaFile, IInputArgument
    {
        public InputFile(string file) : base(file)
        {
        }

        public InputFile(FileInfo file) : base(file)
        {
        }

        public string Name => FileInfo.FullName;
        public string Argument => $"\"{FileInfo.FullName}\"";
    }

    public class OutputFile : MediaFile, IOutputArgument
    {
        public OutputFile(string file) : base(file)
        {
        }

        public OutputFile(FileInfo file) : base(file)
        {
        }

        public string Name => FileInfo.FullName;
        public string Argument => $"\"{FileInfo.FullName}\"";
    }

    public class OutputPipe : IOutputArgument
    {
        public OutputPipe(string pipePath)
        {
            PipePath = pipePath;
        }

        public string PipePath { get; }

        public string Name => PipePath;
        public string Argument => $"\"{PipePath}\"";

    }

    public class InputPipe : IInputArgument
    {
        public InputPipe(string pipePath)
        {
            PipePath = pipePath;
        }

        public string PipePath { get; }

        public string Argument => $"\"{PipePath}\"";
        public MetaData MetaData { get; set; }
    }
}