// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using Crawler.Models;
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
            containerBuilder.RegisterType<MainWindowViewModel>();
            containerBuilder.RegisterType<MainWindowModel>();
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

        #endregion
    }
}
