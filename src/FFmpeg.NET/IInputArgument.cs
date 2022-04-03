namespace FFmpeg.NET
{
    public interface IArgument
    {
        string Argument { get; }
        string Name => Argument;
    }

    public interface IInputArgument : IArgument, IHasMetaData
    {
        public bool UseStandardInput { get { return false; } }
    }

    public interface IOutputArgument : IArgument
    {
    }
}
