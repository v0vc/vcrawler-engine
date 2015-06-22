using Interfaces.API;

namespace Interfaces.Factories
{
    public interface ICommonFactory
    {
        IChannelFactory CreateChannelFactory();

        ICredFactory CreateCredFactory();

        IPlaylistFactory CreatePlaylistFactory();

        ISettingFactory CreateSettingFactory();

        ISubscribeFactory CreateSubscribeFactory();

        ITagFactory CreateTagFactory();

        IVideoItemFactory CreateVideoItemFactory();

        ISqLiteDatabase CreateSqLiteDatabase();

        IYouTubeSite CreateYouTubeSite();

        ITapochekSite CreateTapochekSite();
    }
}
