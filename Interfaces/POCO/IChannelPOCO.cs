namespace Interfaces.POCO
{
    public interface IChannelPOCO
    {
        string ID { get; }
        string Title { get; }
        string SubTitle { get; }
        byte[] Thumbnail { get; }
        string Site { get; }
    }
}
