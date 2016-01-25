// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Autofac;
using DataAPI.Database;
using DataAPI.Trackers;

namespace Models.Factories
{
    public static class CommonFactory
    {
        #region Static Methods

        public static RutrackerSite CreateRutrackerSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<RutrackerSite>();
            }
        }

        public static SqLiteDatabase CreateSqLiteDatabase()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<SqLiteDatabase>();
            }
        }

        public static TapochekSite CreateTapochekSite()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                return scope.Resolve<TapochekSite>();
            }
        }

        #endregion
    }
}
