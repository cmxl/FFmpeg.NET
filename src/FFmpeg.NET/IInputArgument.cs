namespace FFmpeg.NET
{
    public interface IArgument
    {
        string Argument { get; }
#if NETSTANDARD2_1_OR_GREATER
        string Name => Argument;
#else
        string Name { get; }
#endif
    }

    public interface IInputArgument : IArgument, IHasMetaData
    {
#if NETSTANDARD2_1_OR_GREATER
        public bool UseStandardInput { get { return false; } }
#else
        bool UseStandardInput { get; }
#endif
    }

    public interface IOutputArgument : IArgument
    {
    }
}
