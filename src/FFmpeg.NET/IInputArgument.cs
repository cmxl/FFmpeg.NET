namespace FFmpeg.NET
{
    public interface IArgument
    {
        string Argument { get; }
        string Name => Argument;
    }

    public interface IInputArgument : IArgument, IHasMetaData
    {
        bool UseStandardInput { get; }
    }

    public interface IOutputArgument : IArgument
    {
    }
}
