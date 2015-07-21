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

        public string ID { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string ChannelID { get; private set; }
    }
}
