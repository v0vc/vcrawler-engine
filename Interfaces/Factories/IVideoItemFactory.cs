// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IVideoItemFactory
    {
        #region Methods

        IVideoItem CreateVideoItem(SiteType site);

        IVideoItem CreateVideoItem(IVideoItemPOCO poco);

        Task<IVideoItem> GetVideoItemDbAsync(string id);

        Task<IVideoItem> GetVideoItemNetAsync(string id, SiteType site);

        #endregion
    }
}
