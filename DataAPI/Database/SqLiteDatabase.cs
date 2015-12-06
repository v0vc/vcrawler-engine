// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.POCO;
using Interfaces.Models;
using Interfaces.POCO;

namespace DataAPI.Database
{
    public class SqLiteDatabase
    {
        #region Constants

        private const string cookieFolder = "Cookies";
        private const string dbfile = "db.sqlite";
        private const string sqlFile = "sqlite.sql";
        private const string sqlSchemaFolder = "Schema";
        private const string tablechannels = "channels";
        private const string tablechanneltags = "channeltags";
        private const string tablecredentials = "credentials";
        private const string tableitems = "items";
        private const string tableplaylistitems = "playlistitems";
        private const string tableplaylists = "playlists";
        private const string tablesettings = "settings";
        private const string tabletags = "tags";

        #endregion

        #region Static and Readonly Fields

        private readonly string appstartdir;
        private string dbConnection;
        private FileInfo fileBase;

        #region items

        private const string itemId = "id";

        private const string parentID = "parentid";

        private const string title = "title";

        private const string description = "description";

        private const string viewCount = "viewcount";

        private const string duration = "duration";

        private const string comments = "comments";

        private const string thumbnail = "thumbnail";

        private const string timestamp = "timestamp";

        #endregion

        #region channels

        private const string channelId = "id";

        private const string channelTitle = "title";

        private const string channelSubTitle = "subtitle";

        private const string channelThumbnail = "thumbnail";

        private const string channelSite = "site";

        #endregion

        #region channeltags

        private const string tagIdF = "tagid";

        private const string channelIdF = "channelid";

        #endregion

        #region playlists

        private const string playlistID = "id";

        private const string playlistTitle = "title";

        private const string playlistSubTitle = "subtitle";

        private const string playlistThumbnail = "thumbnail";

        private const string playlistChannelId = "channelid";

        #endregion

        #region playlistitems

        private const string fPlaylistId = "playlistid";

        private const string fItemId = "itemid";

        private const string fChannelId = "channelid";

        #endregion

        #region credentials

        private const string credSite = "site";

        private const string credLogin = "login";

        private const string credPass = "pass";

        private const string credCookie = "cookie";

        private const string credExpired = "expired";

        private const string credAutorization = "autorization";

        #endregion

        #region settings

        private const string setKey = "key";

        private const string setVal = "val";

        #endregion

        #region tags

        private const string tagTitle = "title";

        #endregion

        #endregion

        #region Constructors

        public SqLiteDatabase()
        {
            appstartdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (appstartdir == null)
            {
                return;
            }
            string fdb = Path.Combine(appstartdir, dbfile);
            FileBase = new FileInfo(fdb);
            if (!FileBase.Exists)
            {
                CreateDb();
            }
        }

        #endregion

        #region Static Methods

        private static SQLiteCommand GetCommand(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            return new SQLiteCommand { CommandText = sql, CommandType = CommandType.Text };
        }

        #endregion

        #region Methods

        private async void CreateDb()
        {
            string sqliteschema = Path.Combine(appstartdir, sqlSchemaFolder, sqlFile);
            var fnsch = new FileInfo(sqliteschema);
            if (fnsch.Exists)
            {
                string sqltext = File.ReadAllText(fnsch.FullName, Encoding.UTF8);
                await RunSqlCodeAsync(sqltext);
            }

            // now can be set from launch param
            // else
            // {
            //    throw new FileNotFoundException("SQL Scheme not found in " + fnsch.FullName);
            // }
        }

        private async Task RunSqlCodeAsync(string sqltext)
        {
            using (SQLiteCommand command = GetCommand(sqltext))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        private async Task ExecuteNonQueryAsync(SQLiteCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            using (var connection = new SQLiteConnection(dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    await command.ExecuteNonQueryAsync();
                    transaction.Commit();
                }
                connection.Close();
            }
        }

        #endregion

        #region ISqLiteDatabase Members
        /// <summary>
        ///     Файл базы
        /// </summary>
        public FileInfo FileBase
        {
            get
            {
                return fileBase;
            }
            set
            {
                fileBase = value;
                if (fileBase != null)
                {
                    dbConnection =
                        string.Format(
                                      "Data Source={0};Version=3;foreign keys=true;Count Changes=off;Journal Mode=off;Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off",
                            FileBase.FullName);
                }
            }
        }

        /// <summary>
        ///     Удалить канал
        /// </summary>
        /// <param name="parID">ID канала</param>
        /// <returns></returns>
        public async Task DeleteChannelAsync(string parID)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablechannels, channelId, parID);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Удалить тэг у канала
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task DeleteChannelTagsAsync(string channelid, string tag)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}' AND {3}='{4}'", 
                tablechanneltags, 
                channelIdF, 
                channelid, 
                tagIdF, 
                tag);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Удалить credential
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        public async Task DeleteCredAsync(string site)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablecredentials, credSite, site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Удалить элемент канала
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tableitems, itemId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Удалить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        public async Task DeletePlaylistAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tableplaylists, playlistID, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Удалить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        public async Task DeleteSettingAsync(string key)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablesettings, setKey, key);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Получить тэг
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task DeleteTagAsync(string tag)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tabletags, tagTitle, tag);
            await RunSqlCodeAsync(zap);
            zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablechanneltags, tagIdF, tag);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Получить все тэги
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ITagPOCO>> GetAllTagsAsync()
        {
            var res = new List<ITagPOCO>();

            string zap = string.Format(@"SELECT * FROM {0}", tabletags);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var tag = new TagPOCO((string)reader[tagTitle]);
                                res.Add(tag);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить канал по ID
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        public async Task<IChannelPOCO> GetChannelAsync(string id)
        {
            string zap = string.Format(@"SELECT {0},{1},{2},{3} FROM {4} WHERE {5}='{6}'", 
                channelId, 
                channelTitle, 
                channelThumbnail, 
                channelSite, 
                tablechannels, 
                channelId, 
                id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Commit();
                                throw new Exception(zap);
                            }
                            var ch = new ChannelPOCO((string)reader[channelId],
                                (string)reader[channelTitle],
                                (byte[])reader[channelThumbnail],
                                (string)reader[channelSite]);

                            transaction.Commit();
                            return ch;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Получить описание канала из базы
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public async Task<string> GetChannelDescriptionAsync(string channelID)
        {
            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", channelSubTitle, tablechannels, channelId, channelID);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        object res = await command.ExecuteScalarAsync(CancellationToken.None);

                        if (res == null || res == DBNull.Value)
                        {
                            transaction.Commit();
                            return string.Empty;
                        }

                        transaction.Commit();
                        return res as string;
                    }
                }
            }
        }

        /// <summary>
        ///     Получить список видео канала, 0 - все
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int count, int offset)
        {
            var res = new List<IVideoItemPOCO>();

            string zap = count == 0
                ? string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8}='{9}' ORDER BY {6} DESC",
                    itemId,
                    parentID,
                    title,
                    viewCount,
                    duration,
                    comments,
                    timestamp,
                    tableitems,
                    parentID,
                    channelID)
                : string.Format(
                                @"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8}='{9}' ORDER BY {6} DESC LIMIT {10} OFFSET {11}",
                    itemId,
                    parentID,
                    title,
                    viewCount,
                    duration,
                    comments,
                    timestamp,
                    tableitems,
                    parentID,
                    channelID,
                    count,
                    offset);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var vi = new VideoItemPOCO((string)reader[itemId],
                                    (string)reader[parentID],
                                    (string)reader[title],
                                    Convert.ToInt32(reader[viewCount]),
                                    Convert.ToInt32(reader[duration]),
                                    Convert.ToInt32(reader[comments]),
                                    (byte[])reader[thumbnail],
                                    (DateTime)reader[timestamp]);
                                res.Add(vi);
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        ///     Получить количество записей канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<int> GetChannelItemsCountDbAsync(string channelID)
        {
            string zap = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", tableitems, parentID, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        object res = await command.ExecuteScalarAsync(CancellationToken.None);

                        if (res == null || res == DBNull.Value)
                        {
                            transaction.Commit();
                            throw new Exception(zap);
                        }

                        transaction.Commit();
                        return Convert.ToInt32(res);
                    }
                }
            }
        }

        /// <summary>
        ///     Получить список ID видео с канала в базе, 0 - все
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetChannelItemsIdListDbAsync(string channelID, int count, int offset)
        {
            var res = new List<string>();

            string zap = count == 0
                ? string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' ORDER BY {4} DESC",
                    itemId,
                    tableitems,
                    parentID,
                    channelID,
                    timestamp)
                : string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' ORDER BY {4} DESC LIMIT {5} OFFSET {6}",
                    itemId,
                    tableitems,
                    parentID,
                    channelID,
                    timestamp,
                    count,
                    offset);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var vid = reader[itemId] as string;
                                res.Add(vid);
                            }
                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить список всех плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<IEnumerable<IPlaylistPOCO>> GetChannelPlaylistAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tableplaylists, playlistChannelId, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var pl = new PlaylistPOCO((string)reader[playlistID],
                                    (string)reader[playlistTitle],
                                    (string)reader[playlistSubTitle],
                                    (byte[])reader[playlistThumbnail],
                                    (string)reader[playlistChannelId]);
                                res.Add(pl);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        ///     Получить количество плейлистов канала
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public async Task<int> GetChannelPlaylistCountDbAsync(string channelID)
        {
            string zap = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", tableplaylists, playlistChannelId, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        object res = await command.ExecuteScalarAsync(CancellationToken.None);

                        if (res == null || res == DBNull.Value)
                        {
                            transaction.Commit();
                            throw new Exception(zap);
                        }

                        transaction.Commit();
                        return Convert.ToInt32(res);
                    }
                }
            }
        }

        /// <summary>
        ///     Получить список каналов по тэгу
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task<IEnumerable<IChannelPOCO>> GetChannelsByTagAsync(string tag)
        {
            var res = new List<IChannelPOCO>();

            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tablechanneltags, tagIdF, tag);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                IChannelPOCO channel = await GetChannelAsync(reader[channelIdF].ToString());
                                res.Add(channel);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить список всех ID каналов в базе
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetChannelsIdsListDbAsync()
        {
            var res = new List<string>();

            string zap = string.Format(@"SELECT {0} FROM {1}", channelId, tablechannels);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var ch = reader[channelId] as string;
                                res.Add(ch);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить список всех ID плейлистов канала
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetChannelsPlaylistsIdsListDbAsync(string id)
        {
            var res = new List<string>();

            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", playlistID, tableplaylists, playlistChannelId, id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var ch = reader[playlistID] as string;
                                res.Add(ch);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить список всех каналов из бд
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IChannelPOCO>> GetChannelsListAsync()
        {
            var res = new List<IChannelPOCO>();

            string zap = string.Format(@"SELECT {0},{1},{2},{3} FROM {4} ORDER BY {5} ASC", 
                channelId, 
                channelTitle, 
                channelThumbnail, 
                channelSite, 
                tablechannels, 
                channelTitle);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var ch = new ChannelPOCO((string)reader[channelId],
                                    (string)reader[channelTitle],
                                    (byte[])reader[channelThumbnail],
                                    (string)reader[channelSite]);
                                res.Add(ch);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить список тэгов канала
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        public async Task<IEnumerable<ITagPOCO>> GetChannelTagsAsync(string id)
        {
            var res = new List<ITagPOCO>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tablechanneltags, channelIdF, id);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var tag = new TagPOCO((string)reader[tagIdF]);
                                res.Add(tag);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        ///     Получить credentials сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        public async Task<ICredPOCO> GetCredAsync(string site)
        {
            string zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", tablecredentials, credSite, site);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                throw new KeyNotFoundException("No item: " + site);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Commit();
                                throw new Exception(zap);
                            }
                            var cred = new CredPOCO((string)reader[credSite], (string)reader[credLogin], (string)reader[credPass]);
                            transaction.Commit();
                            return cred;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Получить список всех креденшиалов (поддерживаемых площадок)
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ICredPOCO>> GetCredListAsync()
        {
            var res = new List<ICredPOCO>();
            string zap = string.Format("SELECT * FROM {0}", tablecredentials);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var cred = new CredPOCO((string)reader[credSite], (string)reader[credLogin], (string)reader[credPass]);
                                res.Add(cred);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        public async Task<IPlaylistPOCO> GetPlaylistAsync(string id)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tableplaylists, playlistID, id);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Commit();
                                throw new Exception(zap);
                            }
                            var pl = new PlaylistPOCO((string)reader[playlistID],
                                (string)reader[playlistTitle],
                                (string)reader[playlistSubTitle],
                                (byte[])reader[playlistThumbnail],
                                (string)reader[playlistChannelId]);
                            transaction.Commit();
                            return pl;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Получить список видео, относящегося к плэйлисту канала
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID)
        {
            var res = new List<IVideoItemPOCO>();
            var lst = new List<string>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}' AND {3}='{4}'", 
                tableplaylistitems, 
                fPlaylistId, 
                id, 
                fChannelId, 
                channelID);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var r = reader[fItemId] as string;
                                lst.Add(r);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            foreach (string itemid in lst)
            {
                res.Add(await GetVideoItemAsync(itemid));
            }

            return res;
        }

        /// <summary>
        ///     Получение списка ID видео плейлиста из базы
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            var res = new List<string>();

            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", fItemId, tableplaylistitems, fPlaylistId, id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                var vid = reader[fItemId] as string;
                                res.Add(vid);
                            }

                            transaction.Commit();
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        ///     Получить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        public async Task<ISettingPOCO> GetSettingAsync(string key)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tablesettings, setKey, key);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                throw new KeyNotFoundException("No item: " + key);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Commit();
                                throw new Exception(zap);
                            }

                            var cred = new SettingPOCO((string)reader[setKey], (string)reader[setVal]);
                            transaction.Commit();
                            return cred;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Получить видео
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        public async Task<IVideoItemPOCO> GetVideoItemAsync(string id)
        {
            string zap = string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6},{7} FROM {8} WHERE {9}='{10}'",
                itemId,
                parentID,
                title,
                viewCount,
                duration,
                comments,
                thumbnail,
                timestamp,
                tableitems,
                itemId,
                id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            if (!reader.HasRows)
                            {
                                transaction.Commit();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Commit();
                                throw new Exception(zap);
                            }
                            var vi = new VideoItemPOCO((string)reader[itemId],
                                (string)reader[parentID],
                                (string)reader[title],
                                Convert.ToInt32(reader[viewCount]),
                                Convert.ToInt32(reader[duration]),
                                Convert.ToInt32(reader[comments]),
                                (byte[])reader[thumbnail],
                                (DateTime)reader[timestamp]);
                            transaction.Commit();
                            return vi;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Получить описание видео из базы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetVideoItemDescriptionAsync(string id)
        {
            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", description, tableitems, itemId, id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        object res = await command.ExecuteScalarAsync(CancellationToken.None);

                        if (res == null || res == DBNull.Value)
                        {
                            transaction.Commit();
                            return string.Empty;
                        }

                        transaction.Commit();
                        return res as string;
                    }
                }
            }
        }

        /// <summary>
        ///     Записать канал без списка видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public async Task InsertChannelAsync(IChannel channel)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}') VALUES (@{1},@{2},@{3},@{4},@{5})", 
                tablechannels, 
                channelId, 
                channelTitle, 
                channelSubTitle, 
                channelThumbnail, 
                channelSite);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + channelId, channel.ID);
                command.Parameters.AddWithValue("@" + channelTitle, channel.Title);
                command.Parameters.AddWithValue("@" + channelSubTitle, channel.SubTitle);
                command.Parameters.Add("@" + channelThumbnail, DbType.Binary, channel.Thumbnail.Length).Value = channel.Thumbnail;
                command.Parameters.AddWithValue("@" + channelThumbnail, channel.Thumbnail);
                command.Parameters.AddWithValue("@" + channelSite, channel.SiteAdress);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Записать канал со списком видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            await InsertChannelAsync(channel);

            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText =
                            string.Format(
                                          @"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9})",
                                tableitems,
                                itemId,
                                parentID,
                                title,
                                description,
                                viewCount,
                                duration,
                                comments,
                                thumbnail,
                                timestamp);

                        command.CommandType = CommandType.Text;

                        foreach (IVideoItem item in channel.ChannelItems)
                        {
                            command.Parameters.AddWithValue("@" + itemId, item.ID);
                            command.Parameters.AddWithValue("@" + parentID, item.ParentID);
                            command.Parameters.AddWithValue("@" + title, item.Title);
                            command.Parameters.AddWithValue("@" + description, item.Description);
                            command.Parameters.AddWithValue("@" + viewCount, item.ViewCount);
                            command.Parameters.AddWithValue("@" + duration, item.Duration);
                            command.Parameters.AddWithValue("@" + comments, item.Comments);
                            command.Parameters.Add("@" + thumbnail, DbType.Binary, item.Thumbnail.Length).Value = item.Thumbnail;
                            command.Parameters.AddWithValue("@" + timestamp, item.Timestamp);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        ///     Добавить тэг каналу
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task InsertChannelTagsAsync(string channelid, string tag)
        {
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", 
                tablechanneltags, 
                channelIdF, 
                tagIdF);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + channelIdF, channelid);
                command.Parameters.AddWithValue("@" + tagIdF, tag);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Записать credential
        /// </summary>
        /// <param name="cred">Credential</param>
        /// <returns></returns>
        public async Task InsertCredAsync(ICred cred)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6})", 
                tablecredentials, 
                credSite, 
                credLogin, 
                credPass, 
                credCookie, 
                credExpired, 
                credAutorization);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + credSite, cred.SiteAdress);
                command.Parameters.AddWithValue("@" + credLogin, cred.Login);
                command.Parameters.AddWithValue("@" + credPass, cred.Pass);
                command.Parameters.AddWithValue("@" + credCookie, cred.Cookie);
                command.Parameters.AddWithValue("@" + credExpired, cred.Expired);
                command.Parameters.AddWithValue("@" + credAutorization, cred.Autorization);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Добавить видео
        /// </summary>
        /// <param name="item">Видео</param>
        /// <returns></returns>
        public async Task InsertItemAsync(IVideoItem item)
        {
            string zap =
                string.Format(
                              @"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9})",
                    tableitems,
                    itemId,
                    parentID,
                    title,
                    description,
                    viewCount,
                    duration,
                    comments,
                    thumbnail,
                    timestamp);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + itemId, item.ID);
                command.Parameters.AddWithValue("@" + parentID, item.ParentID);
                command.Parameters.AddWithValue("@" + title, item.Title);
                command.Parameters.AddWithValue("@" + description, item.Description);
                command.Parameters.AddWithValue("@" + viewCount, item.ViewCount);
                command.Parameters.AddWithValue("@" + duration, item.Duration);
                command.Parameters.AddWithValue("@" + comments, item.Comments);
                command.Parameters.Add("@" + thumbnail, DbType.Binary, item.Thumbnail.Length).Value = item.Thumbnail;
                command.Parameters.AddWithValue("@" + timestamp, item.Timestamp);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Добавить плэйлист
        /// </summary>
        /// <param name="playlist">Плэйлист</param>
        /// <returns></returns>
        public async Task InsertPlaylistAsync(IPlaylist playlist)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}', '{5}') VALUES (@{1},@{2},@{3},@{4},@{5})", 
                tableplaylists, 
                playlistID, 
                playlistTitle, 
                playlistSubTitle, 
                playlistThumbnail, 
                playlistChannelId);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + playlistID, playlist.ID);
                command.Parameters.AddWithValue("@" + playlistTitle, playlist.Title);
                command.Parameters.AddWithValue("@" + playlistSubTitle, playlist.SubTitle);
                command.Parameters.Add("@" + playlistThumbnail, DbType.Binary, playlist.Thumbnail.Length).Value = playlist.Thumbnail;
                command.Parameters.AddWithValue("@" + playlistChannelId, playlist.ChannelId);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Добавить настройку
        /// </summary>
        /// <param name="setting">Настройка</param>
        /// <returns></returns>
        public async Task InsertSettingAsync(ISetting setting)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", tablesettings, setKey, setVal);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + setKey, setting.Key);
                command.Parameters.AddWithValue("@" + setVal, setting.Value);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Добавить тэг
        /// </summary>
        /// <param name="tag">Тэг</param>
        /// <returns></returns>
        public async Task InsertTagAsync(ITag tag)
        {
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}') VALUES (@{1})", tabletags, tagTitle);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + tagTitle, tag.Title);
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        ///     Переименовать канал
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <param name="newName">Новое название</param>
        /// <returns></returns>
        public async Task RenameChannelAsync(string id, string newName)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablechannels, channelTitle, newName, channelId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Обновить поле требовать авторизацию или нет
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="autorize">0 - не требовать, 1 - требовать</param>
        /// <returns></returns>
        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", 
                tablecredentials, 
                credAutorization, 
                autorize, 
                credSite, 
                site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Обновить логин к сайту
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newlogin">Новый логин</param>
        /// <returns></returns>
        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablecredentials, credLogin, newlogin, credSite, site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Обновить пароль от сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newpassword">новый пароль</param>
        /// <returns></returns>
        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", 
                tablecredentials, 
                credPass, 
                newpassword, 
                credSite, 
                site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Обновить коллекцию видео, относящихся к плэйлисту
        /// </summary>
        /// <param name="playlistid">ID плэйлиста</param>
        /// <param name="itemid">ID видео</param>
        /// <param name="channelid">ID канала</param>
        /// <returns></returns>
        public async Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid)
        {
            // OR IGNORE
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}','{3}') VALUES (@{1},@{2},@{3})", 
                tableplaylistitems, 
                fPlaylistId, 
                fItemId, 
                fChannelId);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + fPlaylistId, playlistid);
                command.Parameters.AddWithValue("@" + fItemId, itemid);
                command.Parameters.AddWithValue("@" + fChannelId, channelid);

                await ExecuteNonQueryAsync(command);
            }

            // zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}' AND {5}='{6}'", Tableplaylistitems,
            // FPlaylistId, playlistid, FItemId, itemid, FChannelId, channelid);
            // using (var command = GetCommand(zap))
            // {
            // await ExecuteNonQueryAsync(command);
            // }
        }

        /// <summary>
        ///     Обновить значение настройки
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <param name="newvalue">Новое значение</param>
        /// <returns></returns>
        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablesettings, setVal, newvalue, setKey, key);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Скукожить базу
        /// </summary>
        /// <returns></returns>
        public async Task VacuumAsync()
        {
            var command = new SQLiteCommand { CommandText = "vacuum", CommandType = CommandType.Text };
            using (var connection = new SQLiteConnection(dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        /// <summary>
        ///     Схранить куки
        /// </summary>
        /// <param name="site"></param>
        /// <param name="cookies"></param>
        public void StoreCookies(string site, CookieContainer cookies)
        {
            if (FileBase.DirectoryName == null)
            {
                throw new Exception("Check db directory");
            }

            var folder = new DirectoryInfo(Path.Combine(FileBase.DirectoryName, cookieFolder));
            if (!folder.Exists)
            {
                folder.Create();
            }
            var fn = new FileInfo(Path.Combine(folder.Name, site));
            if (fn.Exists)
            {
                try
                {
                    fn.Delete();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            using (Stream stream = File.Create(fn.FullName))
            {
                var formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, cookies);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        ///     Получить куки
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public CookieContainer ReadCookies(string site)
        {
            if (FileBase.DirectoryName == null)
            {
                throw new Exception("Check db directory");
            }

            var folder = new DirectoryInfo(Path.Combine(FileBase.DirectoryName, cookieFolder));
            var fn = new FileInfo(Path.Combine(folder.Name, site));
            if (!fn.Exists)
            {
                return null;
            }
            using (Stream stream = File.Open(fn.FullName, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                return (CookieContainer)formatter.Deserialize(stream);
            }
        }

        #endregion
    }
}
