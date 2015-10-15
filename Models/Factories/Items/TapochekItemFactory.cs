// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;

namespace Models.Factories.Items
{
    public class TapochekItemFactory : ICommonItemFactory
    {
        #region ICommonItemFactory Members

        public IVideoItem CreateVideoItem()
        {
            throw new NotImplementedException();
        }

        public IVideoItem CreateVideoItem(IVideoItemPOCO poco)
        {
            throw new NotImplementedException();
        }

        public Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IVideoItem> GetVideoItemNetAsync(string id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
