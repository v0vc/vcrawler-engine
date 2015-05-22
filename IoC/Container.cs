using DataBaseAPI;
using Interfaces.API;
using Interfaces.Factories;
using Models.Factories;
using Ninject;
using Ninject.Extensions.Factory;
using SitesAPI.Videos;

namespace IoC
{
    public class Container
    {
        #region autofac

        //private static IContainer _kernel;

        //public static IContainer Kernel
        //{
        //    get
        //    {
        //        if (_kernel == null)
        //        {
        //            _kernel = GetKernel();
        //        }
        //        return _kernel;
        //    }
        //}

        //private static IContainer GetKernel()
        //{
        //    var builder = new ContainerBuilder();
        //    builder.RegisterType<SqLiteDatabase>().As<ISqLiteDatabase>().SingleInstance();
        //    builder.RegisterType<YouTubeSiteApiV2>().As<IYouTubeSite>().SingleInstance();

        //    builder.RegisterType<SubscribeFactory>().As<ISubscribeFactory>().SingleInstance();
        //    builder.RegisterType<ChannelFactory>().As<IChannelFactory>().SingleInstance();
        //    builder.RegisterType<VideoItemFactory>().As<IVideoItemFactory>().SingleInstance();
        //    builder.RegisterType<TagFactory>().As<ITagFactory>().SingleInstance();
        //    builder.RegisterType<CredFactory>().As<ICredFactory>().SingleInstance();
        //    builder.RegisterType<SettingFactory>().As<ISettingFactory>().SingleInstance();

        //    builder.RegisterType<CommonFactory>().As<ICommonFactory>().SingleInstance();
        //    return builder.Build();
        //}

        #endregion

        private static IKernel _kernel;

        public static IKernel Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    _kernel = GetKernel();
                }
                return _kernel;
            }
        }

        private static IKernel GetKernel()
        {
            var kern = new StandardKernel();
            kern.Bind<ISubscribeFactory>().To<SubscribeFactory>().InSingletonScope();
            kern.Bind<IChannelFactory>().To<ChannelFactory>().InSingletonScope();
            kern.Bind<IVideoItemFactory>().To<VideoItemFactory>().InSingletonScope();
            kern.Bind<IPlaylistFactory>().To<PlaylistFactory>().InSingletonScope();
            kern.Bind<ISqLiteDatabase>().To<SqLiteDatabase>().InSingletonScope();
            kern.Bind<IYouTubeSite>().To<YouTubeSite>().InSingletonScope();
            kern.Bind<ITagFactory>().To<TagFactory>().InSingletonScope();
            kern.Bind<ICredFactory>().To<CredFactory>().InSingletonScope();
            kern.Bind<ISettingFactory>().To<SettingFactory>().InSingletonScope();

            kern.Bind<ICommonFactory>().ToFactory();

            return kern;
        }
    }
}
