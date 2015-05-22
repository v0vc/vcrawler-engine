using Interfaces.Factories;
using Ninject;

namespace Models
{
    public class KerneLocator
    {
        public static ICommonFactory CommonFactory
        {
            get { return IoC.Container.Kernel.Get<ICommonFactory>(); }
        }

        //public static IVideoItemFactory VideoItemFactory
        //{
        //    get { return IoC.Container.Kernel.Get<VideoItemFactory>(); }
        //}

        //public static ISubscribeFactory SubscribeFactory
        //{
        //    get { return IoC.Container.Kernel.Get<SubscribeFactory>(); }
        //}

        //public static IChannelFactory ChannelFactory
        //{
        //    get { return IoC.Container.Kernel.Get<ChannelFactory>(); }
        //}

        //public static IPlaylistFactory PlaylistFactory
        //{
        //    get { return IoC.Container.Kernel.Get<PlaylistFactory>(); }
        //}

        //public static ICredFactory CredFactory
        //{
        //    get { return IoC.Container.Kernel.Get<CredFactory>(); }
        //}

        //public static ISettingFactory SettingFactory
        //{
        //    get { return IoC.Container.Kernel.Get<SettingFactory>(); }
        //}

        //public static SqLiteDatabase SqLiteDatabase
        //{
        //    get { return IoC.Container.Kernel.Get<SqLiteDatabase>(); }
        //}

        //public static YouTubeSiteApiV2 YouTubeSiteApiV2
        //{
        //    get { return IoC.Container.Kernel.Get<YouTubeSiteApiV2>(); }
        //}

        //public static ITagFactory TagFactory
        //{
        //    get { return IoC.Container.Kernel.Get<TagFactory>(); }
        //}
    }
}
