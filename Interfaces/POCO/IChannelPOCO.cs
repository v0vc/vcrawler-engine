namespace Interfaces.POCO
{
    public interface IChannelPOCO
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        byte[] Thumbnail { get; set; }

        string Site { get; set; }
    }
}
