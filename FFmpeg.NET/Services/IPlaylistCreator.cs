using System.Collections.Generic;

namespace FFmpeg.NET.Services
{
    public interface IPlaylistCreator
    {
        string Create(IList<MetaData> metaData);
    }
}