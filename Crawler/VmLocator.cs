// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using Crawler.ViewModels;

namespace Crawler
{
    public class VmLocator
    {
        #region Static and Readonly Fields

        private readonly IContainer container;

        #endregion

        #region Constructors

        public VmLocator()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<MainWindowViewModel>().SingleInstance();
            containerBuilder.RegisterType<EditTagsViewModel>().SingleInstance();
            containerBuilder.RegisterType<AddTagViewModel>().SingleInstance();
            containerBuilder.RegisterType<AddChannelViewModel>().SingleInstance();
            containerBuilder.RegisterType<AddNewTagViewModel>().SingleInstance();
            containerBuilder.RegisterType<DownloadLinkViewModel>().SingleInstance();
            containerBuilder.RegisterType<SettingsViewModel>().SingleInstance();
            container = containerBuilder.Build();
        }

        #endregion

        #region Properties

        public MainWindowViewModel MvViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<MainWindowViewModel>();
                }
            }
        }

        public EditTagsViewModel EditTagViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<EditTagsViewModel>();
                }
            }
        }

        public AddTagViewModel AddTagViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<AddTagViewModel>();
                }
            }
        }

        public AddChannelViewModel AddChannelViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<AddChannelViewModel>();
                }
            }
        }

        public AddNewTagViewModel AddNewTagViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<AddNewTagViewModel>();
                }
            }
        }

        public DownloadLinkViewModel DownloadLinkViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<DownloadLinkViewModel>();
                }
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                using (ILifetimeScope scope = container.BeginLifetimeScope())
                {
                    return scope.Resolve<SettingsViewModel>();
                }
            }
        }
        #endregion
    }
}
