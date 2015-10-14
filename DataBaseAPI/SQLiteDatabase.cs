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
using DataBaseAPI.POCO;
using Interfaces.API;
using Interfaces.Models;
using Interfaces.POCO;

namespace DataBaseAPI
{
    public class SqLiteDatabase : ISqLiteDatabase
    {
        #region Constants

        private const string CookieFolder = "Cookies";
        private const string Dbfile = "db.sqlite";
        private const string SqlFile = "sqlite.sql";
        private const string SqlSchemaFolder = "Schema";
        private const string Tablechannels = "channels";
        private const string Tablechanneltags = "channeltags";
        private const string Tablecredentials = "credentials";
        private const string Tableitems = "items";
        private const string Tableplaylistitems = "playlistitems";
        private const string Tableplaylists = "playlists";
        private const string Tablesettings = "settings";
        private const string Tabletags = "tags";

        #endregion

        #region Static and Readonly Fields

        private readonly string _appstartdir;
        private string _dbConnection;
        private FileInfo fileBase;

        #region items

        public const string ItemId = "id";

        public const string ParentID = "parentid";

        public const string Title = "title";

        public const string Description = "description";

        public const string ViewCount = "viewcount";

        public const string Duration = "duration";

        public const string Comments = "comments";

        public const string Thumbnail = "thumbnail";

        public const string Timestamp = "timestamp";

        #endregion

        #region channels

        public const string ChannelId = "id";

        public const string ChannelTitle = "title";

        public const string ChannelSubTitle = "subtitle";

        public const string ChannelThumbnail = "thumbnail";

        public const string ChannelSite = "site";

        #endregion

        #region channeltags

        private const string TagIdF = "tagid";

        private const string ChannelIdF = "channelid";

        #endregion

        #region playlists

        public const string PlaylistID = "id";

        public const string PlaylistTitle = "title";

        public const string PlaylistSubTitle = "subtitle";

        public const string PlaylistThumbnail = "thumbnail";

        public const string PlaylistChannelId = "channelid";

        #endregion

        #region playlistitems

        private const string FPlaylistId = "playlistid";

        private const string FItemId = "itemid";

        private const string FChannelId = "channelid";

        #endregion

        #region credentials

        public const string CredSite = "site";

        public const string CredLogin = "login";

        public const string CredPass = "pass";

        public const string CredCookie = "cookie";

        public const string CredExpired = "expired";

        public const string CredAutorization = "autorization";

        #endregion

        #region settings

        public const string SetKey = "key";

        public const string SetVal = "val";

        #endregion

        #region tags

        private const string TagTitle = "title";

        #endregion

        #endregion

        #region Constructors

        public SqLiteDatabase()
        {
            _appstartdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_appstartdir == null)
            {
                return;
            }
            string fdb = Path.Combine(_appstartdir, Dbfile);
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
            string sqliteschema = Path.Combine(_appstartdir, SqlSchemaFolder, SqlFile);
            var fnsch = new FileInfo(sqliteschema);
            if (fnsch.Exists)
            {
                string sqltext = File.ReadAllText(fnsch.FullName, Encoding.UTF8);
                await RunSqlCodeAsync(sqltext);
            }
            else
            {
                throw new FileNotFoundException("SQL Scheme not found in " + fnsch.FullName);
            }
        }

        private async Task RunSqlCodeAsync(string sqltext)
        {
            using (SQLiteCommand command = GetCommand(sqltext))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        private async Task<int> ExecuteNonQueryAsync(SQLiteCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                return await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region ISqLiteDatabase Members

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
                    _dbConnection =
                        string.Format(
                                      "Data Source={0};Version=3;foreign keys=true;Count Changes=off;Journal Mode=off;Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off",
                            FileBase.FullName);
                }
            }
        }

        public async Task DeleteChannelAsync(string parentID)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tablechannels, ChannelId, parentID);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteChannelTagsAsync(string channelid, string tag)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}' AND {3}='{4}'", 
                Tablechanneltags, 
                ChannelIdF, 
                channelid, 
                TagIdF, 
                tag);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteCredAsync(string site)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tablecredentials, CredSite, site);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteItemAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tableitems, ItemId, id);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeletePlaylistAsync(string id)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistID, id);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteSettingAsync(string key)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tablesettings, SetKey, key);
            await RunSqlCodeAsync(zap);
        }

        public async Task DeleteTagAsync(string tag)
        {
            string zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tabletags, TagTitle, tag);
            await RunSqlCodeAsync(zap);
            zap = string.Format(@"DELETE FROM {0} WHERE {1}='{2}'", Tablechanneltags, TagIdF, tag);
            await RunSqlCodeAsync(zap);
        }

        public async Task<IEnumerable<ITagPOCO>> GetAllTagsAsync()
        {
            var res = new List<ITagPOCO>();

            string zap = string.Format(@"SELECT * FROM {0}", Tabletags);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var tag = new TagPOCO(reader[TagTitle].ToString());
                            res.Add(tag);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IChannelPOCO> GetChannelAsync(string id)
        {
            string zap = string.Format(@"SELECT {0},{1},{2},{3} FROM {4} WHERE {5}='{6}'", 
                ChannelId, 
                ChannelTitle, 
                ChannelThumbnail, 
                ChannelSite, 
                Tablechannels, 
                ChannelId, 
                id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            throw new KeyNotFoundException("No item: " + id);
                        }

                        if (await reader.ReadAsync())
                        {
                            var ch = new ChannelPOCO(reader);
                            return ch;
                        }
                    }
                }
            }

            throw new Exception(zap);
        }

        public async Task<string> GetChannelDescriptionAsync(string channelID)
        {
            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", ChannelSubTitle, Tablechannels, ChannelId, channelID);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    object res = await command.ExecuteScalarAsync(CancellationToken.None);

                    if (res == null || res == DBNull.Value)
                    {
                        return string.Empty;
                    }

                    return res as string;
                }
            }
        }

        public async Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int count, int offset)
        {
            var res = new List<IVideoItemPOCO>();

            string zap = count == 0
                ? string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8}='{9}' ORDER BY {6} DESC",
                    ItemId,
                    ParentID,
                    Title,
                    ViewCount,
                    Duration,
                    Comments,
                    Timestamp,
                    Tableitems,
                    ParentID,
                    channelID)
                : string.Format(
                                @"SELECT {0},{1},{2},{3},{4},{5},{6} FROM {7} WHERE {8}='{9}' ORDER BY {6} DESC LIMIT {10} OFFSET {11}",
                    ItemId,
                    ParentID,
                    Title,
                    ViewCount,
                    Duration,
                    Comments,
                    Timestamp,
                    Tableitems,
                    ParentID,
                    channelID,
                    count,
                    offset);

            //string zap = count == 0
            //    ? string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6},{7} FROM {8} WHERE {9}='{10}' ORDER BY {7} DESC",
            //        ItemId,
            //        ParentID,
            //        Title,
            //        ViewCount,
            //        Duration,
            //        Comments,
            //        Thumbnail,
            //        Timestamp,
            //        Tableitems,
            //        ParentID,
            //        channelID)
            //    : string.Format(
            //                    @"SELECT {0},{1},{2},{3},{4},{5},{6},{7} FROM {8} WHERE {9}='{10}' ORDER BY {7} DESC LIMIT {11} OFFSET {12}",
            //        ItemId,
            //        ParentID,
            //        Title,
            //        ViewCount,
            //        Duration,
            //        Comments,
            //        Thumbnail,
            //        Timestamp,
            //        Tableitems,
            //        ParentID,
            //        channelID,
            //        count,
            //        offset);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var vi = new VideoItemPOCO(reader);
                            res.Add(vi);
                        }
                    }
                }
            }
            return res;
        }

        public async Task<int> GetChannelItemsCountDbAsync(string channelID)
        {
            string zap = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", Tableitems, ParentID, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    object res = await command.ExecuteScalarAsync(CancellationToken.None);

                    if (res == null || res == DBNull.Value)
                    {
                        throw new Exception(zap);
                    }

                    return Convert.ToInt32(res);
                }
            }
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdListDbAsync(string channelID, int count, int offset)
        {
            var res = new List<string>();

            string zap = count == 0
                ? string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' ORDER BY {4} DESC",
                    ItemId,
                    Tableitems,
                    ParentID,
                    channelID,
                    Timestamp)
                : string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}' ORDER BY {4} DESC LIMIT {5} OFFSET {6}",
                    ItemId,
                    Tableitems,
                    ParentID,
                    channelID,
                    Timestamp,
                    count,
                    offset);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var vid = reader[ItemId] as string;
                            res.Add(vid);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IEnumerable<IPlaylistPOCO>> GetChannelPlaylistAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistChannelId, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var vi = new PlaylistPOCO(reader);
                            res.Add(vi);
                        }
                    }
                }
            }
            return res;
        }

        public async Task<int> GetChannelPlaylistCountDbAsync(string channelID)
        {
            string zap = string.Format(@"SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistChannelId, channelID);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    object res = await command.ExecuteScalarAsync(CancellationToken.None);

                    if (res == null || res == DBNull.Value)
                    {
                        throw new Exception(zap);
                    }

                    return Convert.ToInt32(res);
                }
            }
        }

        public async Task<IEnumerable<IChannelPOCO>> GetChannelsByTagAsync(string tag)
        {
            var res = new List<IChannelPOCO>();

            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tablechanneltags, TagIdF, tag);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            IChannelPOCO channel = await GetChannelAsync(reader[ChannelIdF].ToString());
                            res.Add(channel);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IEnumerable<string>> GetChannelsIdsListDbAsync()
        {
            var res = new List<string>();

            string zap = string.Format(@"SELECT {0} FROM {1}", ChannelId, Tablechannels);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var ch = reader[ChannelId] as string;
                            res.Add(ch);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IEnumerable<IChannelPOCO>> GetChannelsListAsync()
        {
            var res = new List<IChannelPOCO>();

            // var zap = string.Format(@"SELECT * FROM {0} ORDER BY {1} ASC", Tablechannels, ChannelTitle);
            string zap = string.Format(@"SELECT {0},{1},{2},{3} FROM {4} ORDER BY {5} ASC", 
                ChannelId, 
                ChannelTitle, 
                ChannelThumbnail, 
                ChannelSite, 
                Tablechannels, 
                ChannelTitle);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var ch = new ChannelPOCO(reader);
                            res.Add(ch);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IEnumerable<ITagPOCO>> GetChannelTagsAsync(string id)
        {
            var res = new List<ITagPOCO>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tablechanneltags, ChannelIdF, id);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var tag = new TagPOCO(reader[TagIdF].ToString());
                            res.Add(tag);
                        }
                    }
                }
            }
            return res;
        }

        public async Task<ICredPOCO> GetCredAsync(string site)
        {
            string zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablecredentials, CredSite, site);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            throw new KeyNotFoundException("No item: " + site);
                        }

                        if (await reader.ReadAsync())
                        {
                            var cred = new CredPOCO(reader);
                            return cred;
                        }
                    }
                }
            }

            throw new Exception(zap);
        }

        public async Task<IEnumerable<ICredPOCO>> GetCredListAsync()
        {
            var res = new List<ICredPOCO>();
            string zap = string.Format("SELECT * FROM {0}", Tablecredentials);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var cr = new CredPOCO(reader);
                            res.Add(cr);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<IPlaylistPOCO> GetPlaylistAsync(string id)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistID, id);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            throw new KeyNotFoundException("No item: " + id);
                        }

                        if (await reader.ReadAsync())
                        {
                            var pl = new PlaylistPOCO(reader);
                            return pl;
                        }
                    }
                }
            }

            throw new Exception(zap);
        }

        public async Task<IEnumerable<IVideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID)
        {
            var res = new List<IVideoItemPOCO>();
            var lst = new List<string>();
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}' AND {3}='{4}'", 
                Tableplaylistitems, 
                FPlaylistId, 
                id, 
                FChannelId, 
                channelID);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var r = reader[FItemId] as string;
                            lst.Add(r);
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

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            var res = new List<string>();

            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", FItemId, Tableplaylistitems, FPlaylistId, id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            return res;
                        }

                        while (await reader.ReadAsync())
                        {
                            var vid = reader[FItemId] as string;
                            res.Add(vid);
                        }
                    }
                }
            }

            return res;
        }

        public async Task<ISettingPOCO> GetSettingAsync(string key)
        {
            string zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tablesettings, SetKey, key);
            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            throw new KeyNotFoundException("No item: " + key);
                        }

                        if (await reader.ReadAsync())
                        {
                            var cred = new SettingPOCO(reader);
                            return cred;
                        }
                    }
                }
            }

            throw new Exception(zap);
        }

        public async Task<IVideoItemPOCO> GetVideoItemAsync(string id)
        {
            // var zap = string.Format(@"SELECT * FROM {0} WHERE {1}='{2}'", Tableitems, ItemId, id);

            string zap = string.Format(@"SELECT {0},{1},{2},{3},{4},{5},{6},{7} FROM {8} WHERE {9}='{10}'",
                ItemId,
                ParentID,
                Title,
                ViewCount,
                Duration,
                Comments,
                Thumbnail,
                Timestamp,
                Tableitems,
                ItemId,
                id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                        {
                            throw new KeyNotFoundException("No item: " + id);
                        }

                        if (!await reader.ReadAsync())
                        {
                            throw new Exception(zap);
                        }
                        var vi = new VideoItemPOCO(reader);
                        return vi;
                    }
                }
            }
        }

        public async Task<string> GetVideoItemDescriptionAsync(string id)
        {
            string zap = string.Format(@"SELECT {0} FROM {1} WHERE {2}='{3}'", Description, Tableitems, ItemId, id);

            using (SQLiteCommand command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    object res = await command.ExecuteScalarAsync(CancellationToken.None);

                    if (res == null || res == DBNull.Value)
                    {
                        return string.Empty;
                    }

                    return res as string;
                }
            }
        }

        public async Task InsertChannelAsync(IChannel channel)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}') VALUES (@{1},@{2},@{3},@{4},@{5})", 
                Tablechannels, 
                ChannelId, 
                ChannelTitle, 
                ChannelSubTitle, 
                ChannelThumbnail, 
                ChannelSite);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelId, channel.ID);
                command.Parameters.AddWithValue("@" + ChannelTitle, channel.Title);
                command.Parameters.AddWithValue("@" + ChannelSubTitle, channel.SubTitle);
                command.Parameters.Add("@" + ChannelThumbnail, DbType.Binary, channel.Thumbnail.Length).Value = channel.Thumbnail;
                command.Parameters.AddWithValue("@" + ChannelThumbnail, channel.Thumbnail);
                command.Parameters.AddWithValue("@" + ChannelSite, channel.SiteAdress);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            await InsertChannelAsync(channel);

            foreach (IVideoItem item in channel.ChannelItems)
            {
                await InsertItemAsync(item);
            }
        }

        public async Task InsertChannelTagsAsync(string channelid, string tag)
        {
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", 
                Tablechanneltags, 
                ChannelIdF, 
                TagIdF);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelIdF, channelid);
                command.Parameters.AddWithValue("@" + TagIdF, tag);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertCredAsync(ICred cred)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6})", 
                Tablecredentials, 
                CredSite, 
                CredLogin, 
                CredPass, 
                CredCookie, 
                CredExpired, 
                CredAutorization);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + CredSite, cred.Site);
                command.Parameters.AddWithValue("@" + CredLogin, cred.Login);
                command.Parameters.AddWithValue("@" + CredPass, cred.Pass);
                command.Parameters.AddWithValue("@" + CredCookie, cred.Cookie);
                command.Parameters.AddWithValue("@" + CredExpired, cred.Expired);
                command.Parameters.AddWithValue("@" + CredAutorization, cred.Autorization);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertItemAsync(IVideoItem item)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')
                                    VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9})", Tableitems, ItemId, ParentID, Title, Description, ViewCount, Duration, Comments, Thumbnail, Timestamp);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ItemId, item.ID);
                command.Parameters.AddWithValue("@" + ParentID, item.ParentID);
                command.Parameters.AddWithValue("@" + Title, item.Title);
                command.Parameters.AddWithValue("@" + Description, item.Description);
                command.Parameters.AddWithValue("@" + ViewCount, item.ViewCount);
                command.Parameters.AddWithValue("@" + Duration, item.Duration);
                command.Parameters.AddWithValue("@" + Comments, item.Comments);
                command.Parameters.Add("@" + Thumbnail, DbType.Binary, item.Thumbnail.Length).Value = item.Thumbnail;
                command.Parameters.AddWithValue("@" + Timestamp, item.Timestamp);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertPlaylistAsync(IPlaylist playlist)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}', '{5}') VALUES (@{1},@{2},@{3},@{4},@{5})", 
                Tableplaylists, 
                PlaylistID, 
                PlaylistTitle, 
                PlaylistSubTitle, 
                PlaylistThumbnail, 
                PlaylistChannelId);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + PlaylistID, playlist.ID);
                command.Parameters.AddWithValue("@" + PlaylistTitle, playlist.Title);
                command.Parameters.AddWithValue("@" + PlaylistSubTitle, playlist.SubTitle);
                command.Parameters.Add("@" + PlaylistThumbnail, DbType.Binary, playlist.Thumbnail.Length).Value = playlist.Thumbnail;
                command.Parameters.AddWithValue("@" + PlaylistChannelId, playlist.ChannelId);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertSettingAsync(ISetting setting)
        {
            string zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", Tablesettings, SetKey, SetVal);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + SetKey, setting.Key);
                command.Parameters.AddWithValue("@" + SetVal, setting.Value);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertTagAsync(ITag tag)
        {
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}') VALUES (@{1})", Tabletags, TagTitle);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + TagTitle, tag.Title);
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task RenameChannelAsync(string id, string newName)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablechannels, ChannelTitle, newName, ChannelId, id);
            await RunSqlCodeAsync(zap);
        }

        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", 
                Tablecredentials, 
                CredAutorization, 
                autorize, 
                CredSite, 
                site);
            await RunSqlCodeAsync(zap);
        }

        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredLogin, newlogin, CredSite, site);
            await RunSqlCodeAsync(zap);
        }

        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", 
                Tablecredentials, 
                CredPass, 
                newpassword, 
                CredSite, 
                site);
            await RunSqlCodeAsync(zap);
        }

        public async Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid)
        {
            // OR IGNORE
            string zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}','{3}') VALUES (@{1},@{2},@{3})", 
                Tableplaylistitems, 
                FPlaylistId, 
                FItemId, 
                FChannelId);

            using (SQLiteCommand command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + FPlaylistId, playlistid);
                command.Parameters.AddWithValue("@" + FItemId, itemid);
                command.Parameters.AddWithValue("@" + FChannelId, channelid);

                await ExecuteNonQueryAsync(command);
            }

            // zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}' AND {5}='{6}'", Tableplaylistitems,
            // FPlaylistId, playlistid, FItemId, itemid, FChannelId, channelid);
            // using (var command = GetCommand(zap))
            // {
            // await ExecuteNonQueryAsync(command);
            // }
        }

        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            string zap = string.Format(@"UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablesettings, SetVal, newvalue, SetKey, key);
            await RunSqlCodeAsync(zap);
        }

        public async Task VacuumAsync()
        {
            using (SQLiteCommand command = GetCommand("vacuum"))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public void StoreCookies(string site, CookieContainer cookies)
        {
            if (FileBase.DirectoryName == null)
            {
                throw new Exception("Check db directory");
            }

            var folder = new DirectoryInfo(Path.Combine(FileBase.DirectoryName, CookieFolder));
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

        public CookieContainer ReadCookies(string site)
        {
            if (FileBase.DirectoryName == null)
            {
                throw new Exception("Check db directory");
            }

            var folder = new DirectoryInfo(Path.Combine(FileBase.DirectoryName, CookieFolder));
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
