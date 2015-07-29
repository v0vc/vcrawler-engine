using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.POCO;

namespace Interfaces.API
{
    public interface IYouTubeSite
    {
        /// <summary>
        /// Получение заданного количество видео с канала, 0 - все записи
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="maxResult">Количество</param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult);

        /// <summary>
        /// Получение списка популярных видео по стране
        /// </summary>
        /// <param name="regionID">Код региона</param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult);

        /// <summary>
        /// Получение результата запроса
        /// </summary>
        /// <param name="key">Запрос</param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> SearchItemsAsync(string key, string region, int maxResult);

        /// <summary>
        /// Получение видео по ID
        /// </summary>
        /// <param name="videoid">ID видео</param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid);

        /// <summary>
        /// Получение канала по ID
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<IChannelPOCO> GetChannelNetAsync(string channelID);

        /// <summary>
        /// Получение списка плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns>Список плейлистов</returns>
        Task<List<IPlaylistPOCO>> GetChannelPlaylistNetAsync(string channelID);

        /// <summary>
        /// Получение списка видео плэйлиста
        /// </summary>
        /// <param name="link">Ссылка</param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string link);

        /// <summary>
        /// Получение объекта "плэйлист"
        /// </summary>
        /// <param name="id">ID плейлиста</param>
        /// <returns></returns>
        Task<IPlaylistPOCO> GetPlaylistNetAsync(string id);

        /// <summary>
        /// Получение количества видео на канале
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<int> GetChannelItemsCountNetAsync(string channelID);

        /// <summary>
        /// Получить список всех ID видео с канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="maxResult">Количество</param>
        /// <returns></returns>
        Task<List<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult);

        /// <summary>
        /// Получить список всех ID видео плэйлиста
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        Task<List<string>> GetPlaylistItemsIdsListNetAsync(string id);

        /// <summary>
        /// Получение ID канала по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользоватедя</param>
        /// <returns></returns>
        Task<string> GetChannelIdByUserNameNetAsync(string username);

        /// <summary>
        /// Получить облегченный объект видео
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemLiteNetAsync(string id);

        /// <summary>
        /// Получить список облегченных видео по списку id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetVideosListByIdsLiteAsync(List<string> ids);

        /// <summary>
        /// Получить список полных видео по списку id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<List<IVideoItemPOCO>> GetVideosListByIdsAsync(List<string> ids);

        /// <summary>
        /// Получить список похожих каналов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<IChannelPOCO>> GetRelatedChannelsByIdAsync(string id);

        /// <summary>
        /// Получить список субтитров видео
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<IChapterPOCO>> GetVideoSubtitlesByIdAsync(string id);
    }
}
