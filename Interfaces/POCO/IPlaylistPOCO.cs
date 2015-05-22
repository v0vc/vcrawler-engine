namespace Interfaces.POCO
{
    public interface IPlaylistPOCO
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        byte[] Thumbnail { get; set; }

        string ChannelID { get; set; }
    }
}
