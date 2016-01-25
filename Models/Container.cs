// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using DataAPI.Database;
using DataAPI.Trackers;
using DataAPI.Videos;

namespace Models
{
    public static class Container
    {
        #region Static and Readonly Fields

        // #region Static and Readonly Fields

        // private static IKernel _kernel;

        // #endregion

        // #region Static Properties

        // public static IKernel Kernel
        // {
        // get
        // {
        // if (_kernel == null)
        // {
        // _kernel = GetKernel();
        // }
        // return _kernel;
        // }
        // }

        // #endregion

        // #region Static Methods

        // private static IKernel GetKernel()
        // {
        // var kern = new StandardKernel();
        // kern.Bind<IChannelFactory>().To<ChannelFactory>().InSingletonScope();
        // kern.Bind<IVideoItemFactory>().To<VideoItemFactory>().InSingletonScope();
        // kern.Bind<IPlaylistFactory>().To<PlaylistFactory>().InSingletonScope();
        // kern.Bind<ITagFactory>().To<TagFactory>().InSingletonScope();
        // kern.Bind<ICredFactory>().To<CredFactory>().InSingletonScope();
        // kern.Bind<ISettingFactory>().To<SettingFactory>().InSingletonScope();
        // kern.Bind<ISubtitleFactory>().To<SubtitleFactory>().InSingletonScope();
        // kern.Bind<IYouTubeSite>().To<YouTubeSite>().InSingletonScope();
        // kern.Bind<ITapochekSite>().To<TapochekSite>().InSingletonScope();
        // kern.Bind<IRutrackerSite>().To<RutrackerSite>().InSingletonScope();
        // kern.Bind<ISqLiteDatabase>().To<SqLiteDatabase>().InSingletonScope();
        // kern.Bind<ICommonFactory>().ToFactory();
        // return kern;
        // }

        // #endregion
        private static IContainer kernel;

        #endregion

        #region Static Properties

        public static IContainer Kernel
        {
            get
            {
                return kernel ?? (kernel = GetKernel());
            }
        }

        #endregion

        #region Static Methods

        private static IContainer GetKernel()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SqLiteDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<TapochekSite>().AsSelf().SingleInstance();
            builder.RegisterType<RutrackerSite>().AsSelf().SingleInstance();

            return builder.Build();
        }

        #endregion
    }
}
