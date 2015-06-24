using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.API
{
    public interface ITapochekSite
    {
        /// <summary>
        /// Получить куки пользователя с сайта
        /// </summary>
        /// <param name="cred"></param>
        /// <returns></returns>
        Task<CookieCollection> GetCookieNetAsync(ICred cred);

        /// <summary>
        /// Получение заданного количество релизов пользователя, 0 - все релизы
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="maxResult"></param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetUserItemsAsync(string userID, int maxResult);

        /// <summary>
        /// Получение количества релизов пользователя
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        Task<int> GetUserCountItemsAsync(string userID);

        /// <summary>
        /// Получение видео по ID
        /// </summary>
        /// <param name="videoid">ID видео</param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid);
    }
}
