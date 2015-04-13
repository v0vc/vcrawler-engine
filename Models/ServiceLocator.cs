using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI;
using Interfaces.Models;
using Interfaces.Factories;
using Models.Factories;
using Ninject;
using SitesAPI.Videos;

namespace Models
{
    public class ServiceLocator
    {
        public static IVideoItemFactory VideoItemFactory
        {
            get { return IoC.Container.Kernel.Get<VideoItemFactory>(); }
        }

        public static ISubscribeFactory SubscribeFactory
        {
            get { return IoC.Container.Kernel.Get<SubscribeFactory>(); }
        }

        public static IChannelFactory ChannelFactory
        {
            get { return IoC.Container.Kernel.Get<ChannelFactory>(); }
        }

        public static IPlaylistFactory PlaylistFactory
        {
            get { return IoC.Container.Kernel.Get<PlaylistFactory>(); }
        }

        public static ICredFactory CredFactory
        {
            get { return IoC.Container.Kernel.Get<CredFactory>(); }
        }

        public static ISettingFactory SettingFactory
        {
            get { return IoC.Container.Kernel.Get<SettingFactory>(); }
        }

        public static SqLiteDatabase SqLiteDatabase
        {
            get { return IoC.Container.Kernel.Get<SqLiteDatabase>(); }
        }

        public static YouTubeSite YouTubeSite
        {
            get { return IoC.Container.Kernel.Get<YouTubeSite>(); }
        }

        public static TagFactory TagFactory
        {
            get { return IoC.Container.Kernel.Get<TagFactory>(); }
        }
    }
}
