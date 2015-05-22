using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI;
using Interfaces.API;
using Interfaces.Factories;
using SitesAPI.Videos;

namespace Models.Factories
{
    public class CommonFactory : ICommonFactory
    {
        public IChannelFactory CreateChannelFactory()
        {
            return new ChannelFactory(this);
        }

        public ICredFactory CreateCredFactory()
        {
            return new CredFactory(this);
        }

        public IPlaylistFactory CreatePlaylistFactory()
        {
            return new PlaylistFactory(this);
        }

        public ISettingFactory CreateSettingFactory()
        {
            return new SettingFactory(this);
        }

        public ISubscribeFactory CreateSubscribeFactory()
        {
            return new SubscribeFactory(this);
        }

        public ITagFactory CreateTagFactory()
        {
            return new TagFactory(this);
        }

        public IVideoItemFactory CreateVideoItemFactory()
        {
            return new VideoItemFactory(this);
        }

        public ISqLiteDatabase CreateSqLiteDatabase()
        {
            return new SqLiteDatabase();
        }

        public IYouTubeSite CreateYouTubeSite()
        {
            return new YouTubeSiteApiV2();
        }
    }
}
