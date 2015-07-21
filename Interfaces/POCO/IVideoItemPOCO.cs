using System;

namespace Interfaces.POCO
{
    public interface IVideoItemPOCO
    {
        string ID { get; }
        string ParentID { get; }
        string Title { get; }
        string Description { get; }
        long ViewCount { get; }
        int Duration { get; }
        int Comments { get; }
        byte[] Thumbnail { get; }
        DateTime Timestamp { get; }
        string Status { get; }
    }
}
