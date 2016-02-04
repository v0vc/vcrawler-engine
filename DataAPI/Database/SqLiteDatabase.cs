// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;

namespace DataAPI.Database
{
    public class SqLiteDatabase
    {
        #region Constants

        private const string cookieFolder = "Cookies";
        private const string dbfile = "db.sqlite";
        private const string sqlFile = "sqlite.sql";
        private const string sqlSchemaFolder = "Schema";

        #region Items

        private const string itemId = "id";
        private const string parentID = "parentid";
        private const string title = "title";
        private const string description = "description";
        private const string viewCount = "viewcount";
        private const string duration = "duration";
        private const string comments = "comments";
        private const string thumbnail = "thumbnail";
        private const string timestamp = "timestamp";
        private const string syncstate = "syncstate";
        private const string watchstate = "watchstate";

        #endregion

        #region Channels

        private const string channelId = "id";
        private const string channelTitle = "title";
        private const string channelSubTitle = "subtitle";
        private const string channelThumbnail = "thumbnail";
        private const string channelSite = "site";
        private const string newcount = "newcount";
        private const string fastsync = "fastsync";

        #endregion

        #region Channeltags

        private const string tagIdF = "tagid";
        private const string channelIdF = "channelid";

        #endregion

        #region Playlists

        private const string playlistID = "id";
        private const string playlistTitle = "title";
        private const string playlistSubTitle = "subtitle";
        private const string playlistThumbnail = "thumbnail";
        private const string playlistChannelId = "channelid";

        #endregion

        #region PlaylistItems

        private const string fPlaylistId = "playlistid";
        private const string fItemId = "itemid";
        private const string fChannelId = "channelid";

        #endregion

        #region Credentials

        private const string credSite = "site";
        private const string credLogin = "login";
        private const string credPass = "pass";
        private const string credCookie = "cookie";
        private const string credExpired = "expired";
        private const string credAutorization = "autorization";

        #endregion

        #region Settings

        private const string setKey = "key";
        private const string setVal = "val";

        #endregion

        #region Tags

        private const string tagTitle = "title";

        #endregion

        #region Tables

        private const string tablechannels = "channels";
        private const string tablechanneltags = "channeltags";
        private const string tablecredentials = "credentials";
        private const string tableitems = "items";
        private const string tableplaylistitems = "playlistitems";
        private const string tableplaylists = "playlists";
        private const string tablesettings = "settings";
        private const string tabletags = "tags";

        #endregion

        #endregion

        #region Static and Readonly Fields

        private readonly string appstartdir;

        private readonly string playlistItemsString =
            string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}','{3}') VALUES (@{1},@{2},@{3})",
                tableplaylistitems,
                fPlaylistId,
                fItemId,
                fChannelId);

        private readonly string credInsertString =
            string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6})",
                tablecredentials,
                credSite,
                credLogin,
                credPass,
                credCookie,
                credExpired,
                credAutorization);

        private readonly string playlistInsertString =
            string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}', '{5}') VALUES (@{1},@{2},@{3},@{4},@{5})",
                tableplaylists,
                playlistID,
                playlistTitle,
                playlistSubTitle,
                playlistThumbnail,
                playlistChannelId);

        private readonly string channelsSelectString = string.Format(@"SELECT {0},{1},{2},{3},{4},{5} FROM {6}",
                channelId,
                channelTitle,
                channelThumbnail,
                channelSite,
                newcount,
                fastsync,
                tablechannels);

        private readonly string channelsInsertString =
            string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7})",
                tablechannels,
                channelId,
                channelTitle,
                channelSubTitle,
                channelThumbnail,
                channelSite,
                newcount,
                fastsync);

        private readonly string itemsInsertString =
            string.Format(
                          @"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9},@{10},@{11})",
                tableitems,
                itemId,
                parentID,
                title,
                description,
                viewCount,
                duration,
                comments,
                thumbnail,
                timestamp,
                syncstate,
                watchstate);

        private readonly string itemsSelectString = string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6},{7},{8},{9} FROM {10}",
            itemId,
            parentID,
            title,
            viewCount,
            duration,
            comments,
            thumbnail,
            timestamp,
            syncstate,
            watchstate,
            tableitems);

        #endregion

        #region Fields

        private string dbConnection;
        private FileInfo fileBase;

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

        #region Properties

        /// <summary>
        ///     DB file
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

        #endregion

        #region Static Methods

        private static ChannelPOCO CreateChannel(IDataRecord reader)
        {
            return new ChannelPOCO((string)reader[channelId],
                (string)reader[channelTitle],
                (byte[])reader[channelThumbnail],
                (string)reader[channelSite],
                Convert.ToInt32(reader[newcount]),
                Convert.ToBoolean(reader[fastsync]));
        }

        private static CredPOCO CreateCred(IDataRecord reader)
        {
            return new CredPOCO((string)reader[credSite], (string)reader[credLogin], (string)reader[credPass]);
        }

        private static PlaylistPOCO CreatePlaylist(IDataRecord reader)
        {
            return new PlaylistPOCO((string)reader[playlistID],
                (string)reader[playlistTitle],
                (string)reader[playlistSubTitle],
                (byte[])reader[playlistThumbnail],
                (string)reader[playlistChannelId]);
        }

        private static SettingPOCO CreateSetting(IDataRecord reader)
        {
            return new SettingPOCO((string)reader[setKey], (string)reader[setVal]);
        }

        private static TagPOCO CreateTag(IDataRecord reader, string field)
        {
            return new TagPOCO((string)reader[field]);
        }

        private static VideoItemPOCO CreateVideoItem(IDataRecord reader)
        {
            return new VideoItemPOCO((string)reader[itemId],
                (string)reader[parentID],
                (string)reader[title],
                Convert.ToInt32(reader[viewCount]),
                Convert.ToInt32(reader[duration]),
                Convert.ToInt32(reader[comments]),
                (byte[])reader[thumbnail],
                (DateTime)reader[timestamp],
                Convert.ToByte(reader[syncstate]),
                Convert.ToByte(reader[watchstate]));
        }

        private static SQLiteCommand GetCommand(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            return new SQLiteCommand { CommandText = sql, CommandType = CommandType.Text };
        }

        private static async Task InsertPlaylistParam(SQLiteCommand command, IPlaylist playlist)
        {
            command.Parameters.AddWithValue("@" + playlistID, playlist.ID);
            command.Parameters.AddWithValue("@" + playlistTitle, playlist.Title);
            command.Parameters.AddWithValue("@" + playlistSubTitle, playlist.SubTitle);
            if (playlist.Thumbnail != null)
            {
                command.Parameters.Add("@" + playlistThumbnail, DbType.Binary, playlist.Thumbnail.Length).Value =
                    playlist.Thumbnail;
            }
            command.Parameters.AddWithValue("@" + playlistChannelId, playlist.ChannelId);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertChannelParam(SQLiteCommand command, IChannel channel)
        {
            command.Parameters.AddWithValue("@" + channelId, channel.ID);
            command.Parameters.AddWithValue("@" + channelTitle, channel.Title);
            command.Parameters.AddWithValue("@" + channelSubTitle, channel.SubTitle);
            if (channel.Thumbnail != null)
            {
                command.Parameters.Add("@" + channelThumbnail, DbType.Binary, channel.Thumbnail.Length).Value = channel.Thumbnail;
            }
            command.Parameters.AddWithValue("@" + channelSite, EnumHelper.GetAttributeOfType(channel.Site));
            command.Parameters.AddWithValue("@" + newcount, channel.CountNew);
            command.Parameters.AddWithValue("@" + fastsync, channel.UseFast);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertItemParam(SQLiteCommand command, IVideoItem item)
        {
            command.Parameters.AddWithValue("@" + itemId, item.ID);
            command.Parameters.AddWithValue("@" + parentID, item.ParentID);
            command.Parameters.AddWithValue("@" + title, item.Title);
            command.Parameters.AddWithValue("@" + description, item.Description);
            command.Parameters.AddWithValue("@" + viewCount, item.ViewCount);
            command.Parameters.AddWithValue("@" + duration, item.Duration);
            command.Parameters.AddWithValue("@" + comments, item.Comments);
            if (item.Thumbnail != null)
            {
                command.Parameters.Add("@" + thumbnail, DbType.Binary, item.Thumbnail.Length).Value = item.Thumbnail;
            }
            command.Parameters.AddWithValue("@" + timestamp, item.Timestamp);
            command.Parameters.AddWithValue("@" + syncstate, (byte)item.SyncState);
            command.Parameters.AddWithValue("@" + watchstate, (byte)item.WatchState);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task InsertCredParam(SQLiteCommand command, ICred cred)
        {
            command.Parameters.AddWithValue("@" + credSite, cred.SiteAdress);
            command.Parameters.AddWithValue("@" + credLogin, cred.Login);
            command.Parameters.AddWithValue("@" + credPass, cred.Pass);
            command.Parameters.AddWithValue("@" + credCookie, cred.Cookie);
            command.Parameters.AddWithValue("@" + credExpired, cred.Expired);
            command.Parameters.AddWithValue("@" + credAutorization, cred.Autorization);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task UpdatePlaylistItems(SQLiteCommand command, string playlistid, string itemid, string channelid)
        {
            command.Parameters.AddWithValue("@" + fPlaylistId, playlistid);
            command.Parameters.AddWithValue("@" + fItemId, itemid);
            command.Parameters.AddWithValue("@" + fChannelId, channelid);

            await command.ExecuteNonQueryAsync();
        }
        #endregion

        #region Methods

        /// <summary>
        ///     Delete channels
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteChannelsAsync(IEnumerable<string> ids)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        try
                        {
                            foreach (string id in ids)
                            {
                                string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablechannels, channelId, id);
                                command.CommandText = zap;
                                await command.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Delete
        /// </summary>
        /// <param name="parID">channel ID</param>
        /// <returns></returns>
        public async Task DeleteChannelAsync(string parID)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablechannels, channelId, parID);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteChannelPlaylistsAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tableplaylists, playlistChannelId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Delete channel tag
        /// </summary>
        /// <param name="channelid">channel ID</param>
        /// <param name="tag">tag ID</param>
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
        ///     Delete credential
        /// </summary>
        /// <param name="site">site ID</param>
        /// <returns></returns>
        public async Task DeleteCredAsync(string site)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablecredentials, credSite, site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Delete item
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tableitems, itemId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Delete playlist
        /// </summary>
        /// <param name="id">playlist ID</param>
        /// <returns></returns>
        public async Task DeletePlaylistAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tableplaylists, playlistID, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Delete setting
        /// </summary>
        /// <param name="key">setting ID</param>
        /// <returns></returns>
        public async Task DeleteSettingAsync(string key)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablesettings, setKey, key);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Delete tag
        /// </summary>
        /// <param name="tag">tag ID</param>
        /// <returns></returns>
        public async Task DeleteTagAsync(string tag)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tabletags, tagTitle, tag);
            await RunSqlCodeAsync(zap);
            zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", tablechanneltags, tagIdF, tag);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Get all tags
        /// </summary>
        /// <returns></returns>
        public async Task<List<TagPOCO>> GetAllTagsAsync()
        {
            var res = new List<TagPOCO>();

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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                TagPOCO tag = CreateTag(reader, tagTitle);
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
        ///     Get channel by ID
        /// </summary>
        /// <param name="id">channel ID</param>
        /// <returns></returns>
        public async Task<ChannelPOCO> GetChannelAsync(string id)
        {
            string zap = string.Format(@"{0} WHERE {1}='{2}' LIMIT 1", channelsSelectString, channelId, id);

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
                                transaction.Rollback();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                throw new Exception(zap);
                            }
                            ChannelPOCO ch = CreateChannel(reader);
                            transaction.Commit();
                            return ch;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get channel description
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
                            transaction.Rollback();
                            return string.Empty;
                        }

                        transaction.Commit();
                        return res as string;
                    }
                }
            }
        }

        /// <summary>
        ///     Get channel items, 0 - all
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<List<VideoItemPOCO>> GetChannelItemsAsync(string channelID, int count, int offset)
        {
            var res = new List<VideoItemPOCO>();

            string zap = count == 0
                ? string.Format(@"{0} WHERE {1}='{2}' LIMIT 1", itemsSelectString, parentID, channelID)
                : string.Format(@"{0} WHERE {1}='{2}' ORDER BY {3} DESC LIMIT {4} OFFSET {5}",
                    itemsSelectString,
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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                VideoItemPOCO vi = CreateVideoItem(reader);
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
        ///     Get channel items count
        /// </summary>
        /// <param name="channelID">channel ID</param>
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
                            transaction.Rollback();
                            throw new Exception(zap);
                        }

                        transaction.Commit();
                        return Convert.ToInt32(res);
                    }
                }
            }
        }

        /// <summary>
        ///     Get channel count exclude specific state
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="excludeState"></param>
        /// <returns></returns>
        public async Task<int> GetChannelItemsCountDbAsync(string channelID, SyncState excludeState)
        {
            string zap = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}='{2}' AND {3}!='{4}'",
                tableitems,
                parentID,
                channelID,
                syncstate,
                (byte)excludeState);

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
                            transaction.Rollback();
                            throw new Exception(zap);
                        }

                        transaction.Commit();
                        return Convert.ToInt32(res);
                    }
                }
            }
        }

        /// <summary>
        ///     Get channel items ids, except specific state, 0 - all
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <param name="excludedState"></param>
        /// <returns></returns>
        public async Task<List<string>> GetChannelItemsIdListDbAsync(string channelID, int count, int offset, SyncState excludedState)
        {
            var res = new List<string>();

            string zap = count == 0
                ? string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' AND {4}!='{5}' ORDER BY {6} DESC",
                    itemId,
                    tableitems,
                    parentID,
                    channelID,
                    syncstate,
                    (byte)excludedState,
                    timestamp)
                : string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' AND {4}!='{5}' ORDER BY {6} DESC LIMIT {7} OFFSET {8}",
                    itemId,
                    tableitems,
                    parentID,
                    channelID,
                    syncstate,
                    (byte)excludedState,
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
                                transaction.Rollback();
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
        ///     Get channel items ids, 0 - all
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<List<string>> GetChannelItemsIdListDbAsync(string channelID, int count, int offset)
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
                                transaction.Rollback();
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
        ///     Get channel playlists
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public async Task<List<PlaylistPOCO>> GetChannelPlaylistAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();
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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                PlaylistPOCO pl = CreatePlaylist(reader);
                                res.Add(pl);
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            foreach (PlaylistPOCO poco in res)
            {
                poco.PlaylistItems.AddRange(await GetPlaylistItemsIdsListDbAsync(poco.ID));
            }

            return res;
        }

        /// <summary>
        ///     Get channel playlist count
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
                            transaction.Rollback();
                            throw new Exception(zap);
                        }

                        transaction.Commit();
                        return Convert.ToInt32(res);
                    }
                }
            }
        }

        /// <summary>
        ///     Get channel tags
        /// </summary>
        /// <param name="id">channel ID</param>
        /// <returns></returns>
        public async Task<List<TagPOCO>> GetChannelTagsAsync(string id)
        {
            var res = new List<TagPOCO>();
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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                TagPOCO tag = CreateTag(reader, tagIdF);
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
        ///     Get channels by tag
        /// </summary>
        /// <param name="tag">tag ID</param>
        /// <returns></returns>
        public async Task<List<ChannelPOCO>> GetChannelsByTagAsync(string tag)
        {
            var res = new List<ChannelPOCO>();

            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", tablechanneltags, tagIdF, tag);

            var lst = new List<string>();
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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                lst.Add(reader[channelIdF].ToString());
                            }

                            transaction.Commit();
                        }
                    }
                }
            }
            foreach (string id in lst)
            {
                res.Add(await GetChannelAsync(id));
            }

            return res;
        }

        /// <summary>
        ///     Get all channels id's
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetChannelsIdsListDbAsync()
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
                                transaction.Rollback();
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
        ///     Get channels list
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChannelPOCO>> GetChannelsListAsync()
        {
            var res = new List<ChannelPOCO>();

            string zap = string.Format(@"{0} ORDER BY {1} ASC", channelsSelectString, channelTitle);

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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                ChannelPOCO ch = CreateChannel(reader);
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
        ///     Get all playlists id's
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetChannelsPlaylistsIdsListDbAsync(string id)
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
                                transaction.Rollback();
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
        ///     Get site credentials
        /// </summary>
        /// <param name="site">site ID</param>
        /// <returns></returns>
        public async Task<CredPOCO> GetCredAsync(SiteType site)
        {
            string url = EnumHelper.GetAttributeOfType(site);
            string zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}' LIMIT 1", tablecredentials, credSite, url);
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
                                transaction.Rollback();
                                throw new KeyNotFoundException("No item: " + url);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                throw new Exception(zap);
                            }
                            CredPOCO cred = CreateCred(reader);
                            transaction.Commit();
                            return cred;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get credentials
        /// </summary>
        /// <returns></returns>
        public async Task<List<CredPOCO>> GetCredListAsync()
        {
            var res = new List<CredPOCO>();
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
                                transaction.Rollback();
                                return res;
                            }

                            while (await reader.ReadAsync())
                            {
                                CredPOCO cred = CreateCred(reader);
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
        ///     Get playlist
        /// </summary>
        /// <param name="id">playlist ID</param>
        /// <returns></returns>
        public async Task<PlaylistPOCO> GetPlaylistAsync(string id)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}' LIMIT 1", tableplaylists, playlistID, id);
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
                                transaction.Rollback();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                throw new Exception(zap);
                            }
                            PlaylistPOCO pl = CreatePlaylist(reader);
                            transaction.Commit();
                            return pl;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get playlist items
        /// </summary>
        /// <param name="id">playlist ID</param>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public async Task<List<VideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID)
        {
            var res = new List<VideoItemPOCO>();
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
                                transaction.Rollback();
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
        ///     Get playlist items ids
        /// </summary>
        /// <param name="id">playlist ID</param>
        /// <returns></returns>
        public async Task<List<string>> GetPlaylistItemsIdsListDbAsync(string id)
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
                                transaction.Rollback();
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
        ///     Get setting
        /// </summary>
        /// <param name="key">setting ID</param>
        /// <returns></returns>
        public async Task<SettingPOCO> GetSettingAsync(string key)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}' LIMIT 1", tablesettings, setKey, key);
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
                                transaction.Rollback();
                                throw new KeyNotFoundException("No item: " + key);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                throw new Exception(zap);
                            }

                            SettingPOCO cred = CreateSetting(reader);
                            transaction.Commit();
                            return cred;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<VideoItemPOCO> GetVideoItemAsync(string id)
        {
            string zap = string.Format(@"{0} WHERE {1}='{2}' LIMIT 1", itemsSelectString, itemId, id);

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
                                transaction.Rollback();
                                throw new KeyNotFoundException("No item: " + id);
                            }

                            if (!await reader.ReadAsync())
                            {
                                transaction.Rollback();
                                throw new Exception(zap);
                            }
                            VideoItemPOCO vi = CreateVideoItem(reader);
                            transaction.Commit();
                            return vi;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get item description
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetVideoItemDescriptionAsync(string id)
        {
            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' LIMIT 1", description, tableitems, itemId, id);

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
                            transaction.Rollback();
                            return string.Empty;
                        }

                        transaction.Commit();
                        return res as string;
                    }
                }
            }
        }

        /// <summary>
        ///     Insert only channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task InsertChannelAsync(IChannel channel)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = channelsInsertString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            await InsertChannelParam(command, channel);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Full channel insert
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task InsertChannelFullAsync(IChannel channel)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        try
                        {
                            command.CommandType = CommandType.Text;

                            #region channel

                            command.CommandText = channelsInsertString;
                            await InsertChannelParam(command, channel);

                            #endregion

                            #region Playlists

                            command.CommandText = playlistInsertString;
                            foreach (IPlaylist playlist in channel.ChannelPlaylists)
                            {
                                await InsertPlaylistParam(command, playlist);
                            }

                            #endregion

                            #region Items

                            command.CommandText = itemsInsertString;
                            foreach (IVideoItem item in channel.ChannelItems)
                            {
                                await InsertItemParam(command, item);
                            }

                            #endregion

                            #region Update Playlists items

                            command.CommandText = playlistItemsString;
                            foreach (IPlaylist playlist in channel.ChannelPlaylists)
                            {
                                foreach (string plItem in playlist.PlItems)
                                {
                                    await UpdatePlaylistItems(command, playlist.ID, plItem, channel.ID);
                                }
                            }

                            #endregion

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Insert channel with items
        /// </summary>
        /// <param name="channel"></param>
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
                        command.CommandType = CommandType.Text;
                        command.CommandText = itemsInsertString;
                        try
                        {
                            foreach (IVideoItem item in channel.ChannelItems)
                            {
                                await InsertItemParam(command, item);
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Insert only channel items
        /// </summary>
        /// <param name="channelitems"></param>
        /// <returns></returns>
        public async Task InsertChannelItemsAsync(IEnumerable<IVideoItem> channelitems)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = itemsInsertString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            foreach (IVideoItem item in channelitems)
                            {
                                await InsertItemParam(command, item);
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Add tag to channel
        /// </summary>
        /// <param name="channelid">channel ID</param>
        /// <param name="tag">tag ID</param>
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
        ///     Insert credential
        /// </summary>
        /// <param name="cred">Credential</param>
        /// <returns></returns>
        public async Task InsertCredAsync(ICred cred)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = credInsertString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            await InsertCredParam(command, cred);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Insert item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task InsertItemAsync(IVideoItem item)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = itemsInsertString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            await InsertItemParam(command, item);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Insert playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public async Task InsertPlaylistAsync(IPlaylist playlist)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = playlistInsertString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            await InsertPlaylistParam(command, playlist);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Insert setting
        /// </summary>
        /// <param name="setting"></param>
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
        ///     Insert tag
        /// </summary>
        /// <param name="tag"></param>
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
        ///     Get cookie
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public CookieContainer ReadCookies(SiteType site)
        {
            if (FileBase.DirectoryName == null)
            {
                throw new Exception("Check db directory");
            }

            var folder = new DirectoryInfo(Path.Combine(FileBase.DirectoryName, cookieFolder));
            var fn = new FileInfo(Path.Combine(folder.Name, EnumHelper.GetAttributeOfType(site)));
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

        /// <summary>
        ///     Rename channel
        /// </summary>
        /// <param name="id">channel ID</param>
        /// <param name="newName">New name</param>
        /// <returns></returns>
        public async Task RenameChannelAsync(string id, string newName)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablechannels, channelTitle, newName, channelId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Store cookie
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
        ///     Update authorization
        /// </summary>
        /// <param name="site">site ID</param>
        /// <param name="autorize">values 0,1</param>
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
        ///     Update channel NewCount
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task UpdateChannelNewCountAsync(string id, int count)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablechannels, newcount, count, channelId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update channel fast sync option
        /// </summary>
        /// <param name="id"></param>
        /// <param name="useFast"></param>
        /// <returns></returns>
        public async Task UpdateChannelFastSync(string id, bool useFast)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'",
                tablechannels,
                fastsync,
                Convert.ToByte(useFast),
                channelId,
                id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update SyncState, 0-Notset,1-Added,2-Deleted
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task UpdateItemSyncState(string id, SyncState state)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tableitems, syncstate, (byte)state, itemId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update channel items state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="whereState"></param>
        /// <param name="idChannel"></param>
        /// <returns></returns>
        public async Task UpdateItemSyncState(SyncState state, SyncState whereState, string idChannel)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}' AND {1}='{5}'",
                tableitems,
                syncstate,
                (byte)state,
                parentID,
                idChannel,
                (byte)whereState);

            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update SyncState on group of items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task UpdateItemSyncState(IEnumerable<IVideoItem> items, SyncState state)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        try
                        {
                            command.CommandType = CommandType.Text;
                            foreach (IVideoItem item in items)
                            {
                                command.CommandText = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'",
                                    tableitems,
                                    syncstate,
                                    (byte)state,
                                    itemId,
                                    item.ID);
                                await command.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Update WatchState, 0-Notset,1-Watched,2-Planned
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task UpdateItemWatchState(string id, WatchState state)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tableitems, watchstate, (byte)state, itemId, id);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update site login
        /// </summary>
        /// <param name="site">site ID</param>
        /// <param name="newlogin">New login</param>
        /// <returns></returns>
        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablecredentials, credLogin, newlogin, credSite, site);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Update site pass
        /// </summary>
        /// <param name="site">site ID</param>
        /// <param name="newpassword">New pass</param>
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
        ///     Update playlist items references
        /// </summary>
        /// <param name="playlistid">playlist ID</param>
        /// <param name="itemid">item ID</param>
        /// <param name="channelid">channel ID</param>
        /// <returns></returns>
        public async Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid)
        {
            using (var conn = new SQLiteConnection(dbConnection))
            {
                await conn.OpenAsync();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    using (SQLiteCommand command = conn.CreateCommand())
                    {
                        command.CommandText = playlistItemsString;
                        command.CommandType = CommandType.Text;
                        try
                        {
                            await UpdatePlaylistItems(command, playlistid, itemid, channelid);
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Update setting value
        /// </summary>
        /// <param name="key">setting ID</param>
        /// <param name="newvalue">value</param>
        /// <returns></returns>
        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", tablesettings, setVal, newvalue, setKey, key);
            await RunSqlCodeAsync(zap);
        }

        /// <summary>
        ///     Skukojit' db
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
                    command.Transaction = transaction;
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        command.Transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                connection.Close();
            }
        }

        private async Task RunSqlCodeAsync(string sqltext)
        {
            using (SQLiteCommand command = GetCommand(sqltext))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        #endregion
    }
}
