using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.POCO
{
    public interface IPlaylistPOCO
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        string Link { get; set; }

        string ChannelID { get; set; }
    }
}
