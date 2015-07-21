namespace Interfaces.POCO
{
    public interface IPlaylistPOCO
    {
        string ID { get; }
        string Title { get; }
        string SubTitle { get; }
        byte[] Thumbnail { get; }
        string ChannelID { get; }
    }
}
