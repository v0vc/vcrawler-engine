// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.API
{
    public interface ITapochekSite
    {
        #region Methods

        /// <summary>
        ///     Заполнить канал элементами
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        Task FillChannelNetAsync(IChannel channel);

        /// <summary>
        ///     Получение релизов пользователя, 0 - все релизы
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="maxresult"></param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult);

        /// <summary>
        ///     Получить куки пользователя с сайта
        /// </summary>
        /// <param name="cred"></param>
        /// <returns></returns>
        Task<CookieContainer> GetCookieNetAsync(IChannel channel);

        #endregion
    }
}
