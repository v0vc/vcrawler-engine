using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class PlaylistPOCO : IPlaylistPOCO
    {
        public PlaylistPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.PlaylistID] as string;
            Title = reader[SqLiteDatabase.PlaylistTitle] as string;
            SubTitle = reader[SqLiteDatabase.PlaylistSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.PlaylistThumbnail] as byte[];
            ChannelID = reader[SqLiteDatabase.PlaylistChannelId] as string;
        }

        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string ChannelID { get; set; }
    }
}
