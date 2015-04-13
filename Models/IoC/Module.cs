using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI;
using Models.Factories;
using Ninject.Modules;
using SitesAPI.Videos;

namespace Models.IoC
{
    class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<SubscribeFactory>().ToSelf().InSingletonScope();
            Bind<ChannelFactory>().ToSelf().InSingletonScope();
            Bind<VideoItemFactory>().ToSelf().InSingletonScope();
            Bind<PlaylistFactory>().ToSelf().InSingletonScope();
            Bind<SqLiteDatabase>().ToSelf().InSingletonScope();
            Bind<YouTubeSite>().ToSelf().InSingletonScope();
            Bind<TagFactory>().ToSelf().InSingletonScope();
            Bind<CredFactory>().ToSelf().InSingletonScope();
            Bind<SettingFactory>().ToSelf().InSingletonScope();
        }
    }
}
