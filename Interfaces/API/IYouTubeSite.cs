// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.API
{
    public interface IYouTubeSite
    {
        #region Properties

        ICred Cred { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Получение ID канала по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользоватедя</param>
        /// <returns></returns>
        Task<string> GetChannelIdByUserNameNetAsync(string username);

        /// <summary>
        ///     Получение заданного количество видео с канала, 0 - все записи
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="maxResult">Количество</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult);

        /// <summary>
        ///     Получение количества видео на канале
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<int> GetChannelItemsCountNetAsync(string channelID);

        /// <summary>
        ///     Получить список всех ID видео с канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="maxResult">Количество</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult);

        /// <summary>
        ///     Получение канала по ID
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<IChannelPOCO> GetChannelNetAsync(string channelID);

        /// <summary>
        ///     Получение списка плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns>Список плейлистов</returns>
        Task<IEnumerable<IPlaylistPOCO>> GetChannelPlaylistNetAsync(string channelID);

        /// <summary>
        ///     Получить список всех ID видео плэйлиста
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync(string id);

        /// <summary>
        ///     Получение списка видео плэйлиста
        /// </summary>
        /// <param name="link">Ссылка</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string link);

        /// <summary>
        ///     Получение объекта "плэйлист"
        /// </summary>
        /// <param name="id">ID плейлиста</param>
        /// <returns></returns>
        Task<IPlaylistPOCO> GetPlaylistNetAsync(string id);

        /// <summary>
        ///     Получение списка популярных видео по стране
        /// </summary>
        /// <param name="regionID">Код региона</param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult);

        /// <summary>
        ///     Получить список похожих каналов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<IChannelPOCO>> GetRelatedChannelsByIdAsync(string id);

        /// <summary>
        ///     Получить облегченный объект видео
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemLiteNetAsync(string id);

        /// <summary>
        ///     Получение видео по ID
        /// </summary>
        /// <param name="videoid">ID видео</param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid);

        /// <summary>
        ///     Получить список субтитров видео
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<IChapterPOCO>> GetVideoSubtitlesByIdAsync(string id);

        /// <summary>
        ///     Получить список полных видео по списку id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetVideosListByIdsAsync(List<string> ids);

        /// <summary>
        ///     Получить список облегченных видео по списку id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetVideosListByIdsLiteAsync(List<string> ids);

        /// <summary>
        ///     Парсим что ввел юзер для получения ID канала
        /// </summary>
        /// <param name="inputChannelLink"></param>
        /// <returns></returns>
        Task<string> ParseChannelLink(string inputChannelLink);

        /// <summary>
        ///     Получение результата запроса
        /// </summary>
        /// <param name="key">Запрос</param>
        /// <param name="region"></param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> SearchItemsAsync(string key, string region, int maxResult);

        #endregion
    }
}
