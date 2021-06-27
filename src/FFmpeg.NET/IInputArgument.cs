namespace FFmpeg.NET
{
    public interface IArgument
    {
        string Argument { get; }
        string Name => Argument;
    }

    public interface IInputArgument : IArgument, IHasMetaData
    {
    }

    public interface IOutputArgument : IArgument
    {
    }
}
