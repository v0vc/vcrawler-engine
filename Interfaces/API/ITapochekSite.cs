using System.Collections.Generic;
using System.Net;
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
        /// Получение релизов пользователя, 0 - все релизы
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="maxresult"></param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult);

        /// <summary>
        /// Получение видео по ID
        /// </summary>
        /// <param name="videoid">ID видео</param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid);

        /// <summary>
        /// Получить канал по ID
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IChannelPOCO> GetChannelNetAsync(CookieCollection cookie, string id);
    }
}
