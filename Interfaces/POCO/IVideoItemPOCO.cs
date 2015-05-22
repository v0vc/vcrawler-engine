using System;

namespace Interfaces.POCO
{
    public interface IVideoItemPOCO
    {
        string ID { get; set; }

        string ParentID { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        int ViewCount { get; set; }

        int Duration { get; set; }

        int Comments { get; set; }

        byte[] Thumbnail { get; set; }

        DateTime Timestamp { get; set; }

        string Status { get; set; }
    }
}
