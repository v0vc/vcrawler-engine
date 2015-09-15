// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IChannelFactory
    {
        #region Methods

        IChannel CreateChannel();

        IChannel CreateChannel(IChannelPOCO poco);

        Task<IChannel> GetChannelDbAsync(string channelID);

        Task<string> GetChannelIdByUserNameNetAsync(string username);

        Task<IChannel> GetChannelNetAsync(string channelID);

        #endregion
    }
}
