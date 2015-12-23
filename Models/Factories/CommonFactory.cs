// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using DataAPI.Database;
using DataAPI.Trackers;
using DataAPI.Videos;

namespace Models.Factories
{
    public static class CommonFactory
    {
        #region Methods

        public static ChannelFactory CreateChannelFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ChannelFactory>();
            }
        }

        public static CredFactory CreateCredFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<CredFactory>();
            }
        }

        public static PlaylistFactory CreatePlaylistFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<PlaylistFactory>();
            }
        }

        public static RutrackerSite CreateRutrackerSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<RutrackerSite>();
            }
        }

        public static SettingFactory CreateSettingFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SettingFactory>();
            }
        }

        public static SqLiteDatabase CreateSqLiteDatabase()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SqLiteDatabase>();
            }
        }

        public static SubtitleFactory CreateSubtitleFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SubtitleFactory>();
            }
        }

        public static TagFactory CreateTagFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<TagFactory>();
            }
        }

        public static TapochekSite CreateTapochekSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<TapochekSite>();
            }
        }

        public static VideoItemFactory CreateVideoItemFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<VideoItemFactory>();
            }
        }

        public static YouTubeSite CreateYouTubeSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<YouTubeSite>();
            }
        }

        #endregion
    }
}
