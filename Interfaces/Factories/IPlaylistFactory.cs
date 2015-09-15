// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IPlaylistFactory
    {
        #region Methods

        IPlaylist CreatePlaylist();

        IPlaylist CreatePlaylist(IPlaylistPOCO poco);

        Task<IPlaylist> GetPlaylistDbAsync(string id);

        Task<IPlaylist> GetPlaylistNetAsync(string id);

        #endregion
    }
}
