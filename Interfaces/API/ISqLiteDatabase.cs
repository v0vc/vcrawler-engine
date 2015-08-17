using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.API
{
    public interface ISqLiteDatabase
    {
        /// <summary>
        /// Файл базы
        /// </summary>
        FileInfo FileBase { get; set; }

        /// <summary>
        /// Получить канал по ID
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        Task<IChannelPOCO> GetChannelAsync(string id);

        /// <summary>
        /// Получить список всех каналов из бд
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IChannelPOCO>> GetChannelsListAsync();

        /// <summary>
        /// Записать канал без списка видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        Task InsertChannelAsync(IChannel channel);

        /// <summary>
        /// Удалить канал
        /// </summary>
        /// <param name="parentID">ID канала</param>
        /// <returns></returns>
        Task DeleteChannelAsync(string parentID);

        /// <summary>
        /// Переименовать канал
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <param name="newName">Новое название</param>
        /// <returns></returns>
        Task RenameChannelAsync(string id, string newName);

        /// <summary>
        /// Записать канал со списком видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        Task InsertChannelItemsAsync(IChannel channel);

        /// <summary>
        /// Получить видео
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        Task<IVideoItemPOCO> GetVideoItemAsync(string id);

        /// <summary>
        /// Получить список всех видео канала
        /// </summary>
        /// <param name="parentID">ID канала</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(string parentID);

        /// <summary>
        /// Записать видео
        /// </summary>
        /// <param name="item">Видео</param>
        /// <returns></returns>
        Task InsertItemAsync(IVideoItem item);

        /// <summary>
        /// Удалить видео
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        Task DeleteItemAsync(string id);

        /// <summary>
        /// Записать плэйлист
        /// </summary>
        /// <param name="playlist">Плэйлист</param>
        /// <returns></returns>
        Task InsertPlaylistAsync(IPlaylist playlist);

        /// <summary>
        /// Удалить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        Task DeletePlaylistAsync(string id);

        /// <summary>
        /// Получить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        Task<IPlaylistPOCO> GetPlaylistAsync(string id);

        /// <summary>
        /// Обновить коллекцию видео, относящихся к плэйлисту
        /// </summary>
        /// <param name="playlistid">ID плэйлиста</param>
        /// <param name="itemid">ID видео</param>
        /// <param name="channelid">ID канала</param>
        /// <returns></returns>
        Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid);

        /// <summary>
        /// Получить список видео, относящегося к плэйлисту канала
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<IEnumerable<IVideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID);

        /// <summary>
        /// Получить список всех плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<IEnumerable<IPlaylistPOCO>> GetChannelPlaylistAsync(string channelID);

        /// <summary>
        /// Записать тэг
        /// </summary>
        /// <param name="tag">Тэг</param>
        /// <returns></returns>
        Task InsertTagAsync(ITag tag);

        /// <summary>
        /// Получить тэг
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        Task DeleteTagAsync(string tag);

        /// <summary>
        /// Добавить тэг каналу
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        Task InsertChannelTagsAsync(string channelid, string tag);

        /// <summary>
        /// Удалить тэг у канала
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        Task DeleteChannelTagsAsync(string channelid, string tag);

        /// <summary>
        /// Получить список каналов по тэгу
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        Task<IEnumerable<IChannelPOCO>> GetChannelsByTagAsync(string tag);

        /// <summary>
        /// Получить список тэгов канала
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        Task<IEnumerable<ITagPOCO>> GetChannelTagsAsync(string id);

        /// <summary>
        /// Получить все тэги
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ITagPOCO>> GetAllTagsAsync();

        /// <summary>
        /// Получить список всех креденшиалов (поддерживаемых площадок)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ICredPOCO>> GetCredListAsync();

        /// <summary>
        /// Получить credentials сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        Task<ICredPOCO> GetCredAsync(string site);

        /// <summary>
        /// Записать credential
        /// </summary>
        /// <param name="cred">Credential</param>
        /// <returns></returns>
        Task InsertCredAsync(ICred cred);

        /// <summary>
        /// Удалить credential
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        Task DeleteCredAsync(string site);

        /// <summary>
        /// Обновить логин к сайту
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newlogin">Новый логин</param>
        /// <returns></returns>
        Task UpdateLoginAsync(string site, string newlogin);

        /// <summary>
        /// Обновить пароль от сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newpassword">новый пароль</param>
        /// <returns></returns>
        Task UpdatePasswordAsync(string site, string newpassword);

        /// <summary>
        /// Обновить куку
        /// </summary>
        /// <param name="site"></param>
        /// <param name="newcookie"></param>
        /// <returns></returns>
        Task UpdateCookieAsync(string site, string newcookie);

        /// <summary>
        /// Обновить пасскей
        /// </summary>
        /// <param name="site"></param>
        /// <param name="newexpired"></param>
        /// <returns></returns>
        Task UpdateExpiredAsync(string site, DateTime newexpired);

        /// <summary>
        /// Обновить поле требовать авторизацию или нет
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="autorize">0 - не требовать, 1 - требовать</param>
        /// <returns></returns>
        Task UpdateAutorizationAsync(string site, short autorize);

        /// <summary>
        /// Записать настройку
        /// </summary>
        /// <param name="setting">Настройка</param>
        /// <returns></returns>
        Task InsertSettingAsync(ISetting setting);

        /// <summary>
        /// Удалить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        Task DeleteSettingAsync(string key);

        /// <summary>
        /// Обновить значение настройки
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <param name="newvalue">Новое значение</param>
        /// <returns></returns>
        Task UpdateSettingAsync(string key, string newvalue);

        /// <summary>
        /// Получить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        Task<ISettingPOCO> GetSettingAsync(string key);

        /// <summary>
        /// Получить количество записей канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        Task<int> GetChannelItemsCountDbAsync(string channelID);

        /// <summary>
        /// Получение списка ID видео плейлиста из базы
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync(string id);

        /// <summary>
        /// Получить список всех ID каналов в базе
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetChannelsIdsListDbAsync();

        /// <summary>
        /// Получить список ID видео с канала в базе
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetChannelItemsIdListDbAsync(string channelID);

        /// <summary>
        /// Скукожить базу
        /// </summary>
        /// <returns></returns>
        Task VacuumAsync();
    }
}
