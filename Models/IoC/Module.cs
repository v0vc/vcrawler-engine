using DataBaseAPI;
using Interfaces.API;
using Interfaces.Factories;
using Models.Factories;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using SitesAPI.Videos;

namespace Models.IoC
{
    class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ISubscribeFactory>().To<SubscribeFactory>().InSingletonScope();
            Bind<IChannelFactory>().To<ChannelFactory>().InSingletonScope();
            Bind<IVideoItemFactory>().To<VideoItemFactory>().InSingletonScope();
            Bind<IPlaylistFactory>().To<PlaylistFactory>().InSingletonScope();
            Bind<ISqLiteDatabase>().To<SqLiteDatabase>().InSingletonScope();
            Bind<IYouTubeSite>().To<YouTubeSiteApiV2>().InSingletonScope();
            Bind<ITagFactory>().To<TagFactory>().InSingletonScope();
            Bind<ICredFactory>().To<CredFactory>().InSingletonScope();
            Bind<ISettingFactory>().To<SettingFactory>().InSingletonScope();

            Bind<ICommonFactory>().ToFactory();
        }
    }
}
