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
    public class CommonFactory
    {
        #region Methods

        public ChannelFactory CreateChannelFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ChannelFactory>();
            }
        }

        public CredFactory CreateCredFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<CredFactory>();
            }
        }

        public PlaylistFactory CreatePlaylistFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<PlaylistFactory>();
            }
        }

        public RutrackerSite CreateRutrackerSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<RutrackerSite>();
            }
        }

        public SettingFactory CreateSettingFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SettingFactory>();
            }
        }

        public SqLiteDatabase CreateSqLiteDatabase()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SqLiteDatabase>();
            }
        }

        public SubtitleFactory CreateSubtitleFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SubtitleFactory>();
            }
        }

        public TagFactory CreateTagFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<TagFactory>();
            }
        }

        public TapochekSite CreateTapochekSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<TapochekSite>();
            }
        }

        public VideoItemFactory CreateVideoItemFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<VideoItemFactory>();
            }
        }

        public YouTubeSite CreateYouTubeSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<YouTubeSite>();
            }
        }

        #endregion
    }
}
