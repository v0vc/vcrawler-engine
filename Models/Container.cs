// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using DataAPI.Database;
using DataAPI.Trackers;
using DataAPI.Videos;
using Models.Factories;

namespace Models
{
    public class Container
    {
        //#region Static and Readonly Fields

        //private static IKernel _kernel;

        //#endregion

        //#region Static Properties

        //public static IKernel Kernel
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

        //#endregion

        //#region Static Methods

        //private static IKernel GetKernel()
        //{
        //    var kern = new StandardKernel();
        //    kern.Bind<IChannelFactory>().To<ChannelFactory>().InSingletonScope();
        //    kern.Bind<IVideoItemFactory>().To<VideoItemFactory>().InSingletonScope();
        //    kern.Bind<IPlaylistFactory>().To<PlaylistFactory>().InSingletonScope();
        //    kern.Bind<ITagFactory>().To<TagFactory>().InSingletonScope();
        //    kern.Bind<ICredFactory>().To<CredFactory>().InSingletonScope();
        //    kern.Bind<ISettingFactory>().To<SettingFactory>().InSingletonScope();
        //    kern.Bind<ISubtitleFactory>().To<SubtitleFactory>().InSingletonScope();
        //    kern.Bind<IYouTubeSite>().To<YouTubeSite>().InSingletonScope();
        //    kern.Bind<ITapochekSite>().To<TapochekSite>().InSingletonScope();
        //    kern.Bind<IRutrackerSite>().To<RutrackerSite>().InSingletonScope();
        //    kern.Bind<ISqLiteDatabase>().To<SqLiteDatabase>().InSingletonScope();
        //    kern.Bind<ICommonFactory>().ToFactory();
        //    return kern;
        //}

        //#endregion

        private static IContainer _kernel;

        public static IContainer Kernel
        {
            get
            {
                return _kernel ?? (_kernel = GetKernel());
            }
        }

        private static IContainer GetKernel()
        {
            var builder = new ContainerBuilder();
            //builder.RegisterType<SqLiteDatabase>().As<ISqLiteDatabase>().SingleInstance();
            builder.RegisterType<SqLiteDatabase>().AsSelf().SingleInstance();
            //builder.RegisterType<YouTubeSite>().As<IYouTubeSite>().SingleInstance();
            builder.RegisterType<YouTubeSite>().AsSelf().SingleInstance();
            //builder.RegisterType<TapochekSite>().As<ITapochekSite>().SingleInstance();
            builder.RegisterType<TapochekSite>().AsSelf().SingleInstance();
            //builder.RegisterType<RutrackerSite>().As<IRutrackerSite>().SingleInstance();
            builder.RegisterType<RutrackerSite>().AsSelf().SingleInstance();

            //builder.RegisterType<ChannelFactory>().As<IChannelFactory>().SingleInstance();
            builder.RegisterType<ChannelFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<VideoItemFactory>().As<IVideoItemFactory>().SingleInstance();
            builder.RegisterType<VideoItemFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<PlaylistFactory>().As<IPlaylistFactory>().SingleInstance();
            builder.RegisterType<PlaylistFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<TagFactory>().As<ITagFactory>().SingleInstance();
            builder.RegisterType<TagFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<CredFactory>().As<ICredFactory>().SingleInstance();
            builder.RegisterType<CredFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<SettingFactory>().As<ISettingFactory>().SingleInstance();
            builder.RegisterType<SettingFactory>().AsSelf().SingleInstance();
            //builder.RegisterType<SubtitleFactory>().As<ISubtitleFactory>().SingleInstance();
            builder.RegisterType<SubtitleFactory>().AsSelf().SingleInstance();

            //builder.RegisterType<CommonFactory>().As<ICommonFactory>().SingleInstance();
            builder.RegisterType<CommonFactory>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
