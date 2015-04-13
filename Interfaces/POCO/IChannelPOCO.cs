using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace Interfaces.POCO
{
    public interface IChannelPOCO
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        DateTime LastUpdated { get; set; }

        byte[] Thumbnail { get; set; }

        string Site { get; set; }
    }
}
