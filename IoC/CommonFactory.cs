// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using Interfaces.API;
using Interfaces.Factories;

namespace IoC
{
    public class CommonFactory : ICommonFactory
    {
        #region ICommonFactory Members

        public IChannelFactory CreateChannelFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<IChannelFactory>();
            }
        }

        public ISubtitleFactory CreateSubtitleFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ISubtitleFactory>();
            }
        }

        public ICredFactory CreateCredFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ICredFactory>();
            }
        }

        public IPlaylistFactory CreatePlaylistFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<IPlaylistFactory>();
            }
        }

        public IRutrackerSite CreateRutrackerSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<IRutrackerSite>();
            }
        }

        public ISettingFactory CreateSettingFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ISettingFactory>();
            }
        }

        public ISqLiteDatabase CreateSqLiteDatabase()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ISqLiteDatabase>();
            }
        }

        public ITagFactory CreateTagFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ITagFactory>();
            }
        }

        public ITapochekSite CreateTapochekSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<ITapochekSite>();
            }
        }

        public IVideoItemFactory CreateVideoItemFactory()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<IVideoItemFactory>();
            }
        }

        public IYouTubeSite CreateYouTubeSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<IYouTubeSite>();
            }
        }

        #endregion
    }
}
