using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI.POCO;
using Interfaces.Models;
using Interfaces.POCO;

namespace DataBaseAPI
{
    public class SqLiteDatabase
    {
        private const string Dbfile = "db.sqlite";

        private const string SqlSchemaFolder = "Schema";

        private const string SqlFile = "sqlite.sql";

        private readonly string _dbConnection;

        private readonly string _appstartdir;

        #region tables

        private const string Tablechannels = "channels";

        private const string Tablechanneltags = "channeltags";

        private const string Tabletags = "tags";

        private const string Tablecredentials = "credentials";

        private const string Tableitems = "items";

        private const string Tableplaylists = "playlists";

        private const string Tableplaylistitems = "playlistitems";

        private const string Tablesettings = "settings";

        #endregion

        #region items

        public static readonly string ItemId = "id";

        public static readonly string ParentID = "parentid";

        public static readonly string Title = "title";

        public static readonly string Description = "description";

        public static readonly string ViewCount = "viewcount";

        public static readonly string Duration = "duration";

        public static readonly string Comments = "comments";

        public static readonly string Thumbnail = "thumbnail";

        public static readonly string Timestamp = "timestamp";

        #endregion

        #region channels

        public static readonly string ChannelId = "id";

        public static readonly string ChannelTitle = "title";

        public static readonly string ChannelSubTitle = "subtitle";

        public static readonly string ChannelLastUpdated = "lastupdated";

        public static readonly string ChannelThumbnail = "thumbnail";

        public static readonly string ChannelSite = "site";

        #endregion

        #region tags

        public static readonly string TagTitle = "title";

        #endregion

        #region channeltags

        public static readonly string TagIdF = "tagid";

        public static readonly string ChannelIdF = "channelid";

        #endregion

        #region playlists

        public static readonly string PlaylistID = "id";

        public static readonly string PlaylistTitle = "title";

        public static readonly string PlaylistSubTitle = "subtitle";

        public static readonly string PlaylistLink = "link";

        public static readonly string PlaylistChannelId = "channelid";

        #endregion

        #region playlistitems

        public static readonly string FPlaylistId = "playlistid";

        public static readonly string FItemId = "itemid";

        public static readonly string FChannelId = "channelid";

        #endregion

        #region credentials

        public static readonly string CredSite = "site";

        public static readonly string CredLogin = "login";

        public static readonly string CredPass = "pass";

        public static readonly string CredCookie = "cookie";

        public static readonly string CredPasskey = "passkey";

        public static readonly string CredAutorization = "autorization";

        #endregion

        #region settings

        public static readonly string SetKey = "key";

        public static readonly string SetVal = "val";

        #endregion

        public SqLiteDatabase()
        {
            _appstartdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (_appstartdir == null) return;
            var fdb = Path.Combine(_appstartdir, Dbfile);
            var fndb = new FileInfo(fdb);
            _dbConnection = String.Format("Data Source={0};Version=3;foreign keys=true;Count Changes=off;Journal Mode=off;Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off", fndb.FullName);
            if (!fndb.Exists)
            {
                CreateDb();
                //Task.Run(() => CreateDb());
                //var t = new Task(CreateDb);
                //t.RunSynchronously();
                //t.Wait();
            }
        }

        private async Task<int> ExecuteNonQueryAsync(SQLiteCommand command)
        {
            if (command == null) 
                throw new ArgumentNullException("command");

            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                return await command.ExecuteNonQueryAsync();
            }
        }

        private static SQLiteCommand GetCommand(string sql)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql");

            return new SQLiteCommand { CommandText = sql, CommandType = CommandType.Text };
        }

        public async void CreateDb()
        {
            var sqliteschema = Path.Combine(_appstartdir, SqlSchemaFolder, SqlFile);
            var fnsch = new FileInfo(sqliteschema);
            if (fnsch.Exists)
            {
                var sqltext = File.ReadAllText(fnsch.FullName, Encoding.UTF8);
                using (var command = GetCommand(sqltext))
                {
                    await ExecuteNonQueryAsync(command);
                }
            }
            else
                throw new FileNotFoundException("SQL Scheme not found in " + fnsch.FullName);
        }

        /// <summary>
        /// Получить канал
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        public async Task<ChannelPOCO> GetChannelAsync(string id)
        {
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablechannels, ChannelId, id);
            using (var command = GetCommand(zap))
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (!reader.HasRows)
                        throw new KeyNotFoundException("No item: " + id);

                    if (await reader.ReadAsync())
                    {
                        var ch = new ChannelPOCO(reader);
                        return ch;
                    }
                }
            }

            throw new Exception(zap);
        }

        /// <summary>
        /// Получить список всех каналов из бд
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChannelPOCO>> GetChannelsListAsync()
        {
            var res = new List<ChannelPOCO>();

            var zap = string.Format("SELECT * FROM {0}", Tablechannels);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

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

        /// <summary>
        /// Записать канал без списка видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public async Task InsertChannelAsync(IChannel channel)
        {
            var zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}', '{4}', '{5}', '{6}') VALUES (@{1},@{2},@{3}, @{4}, @{5},@{6})",
                Tablechannels,
                ChannelId,
                ChannelTitle,
                ChannelSubTitle,
                ChannelLastUpdated,
                ChannelThumbnail,
                ChannelSite
                );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelId, channel.ID);
                command.Parameters.AddWithValue("@" + ChannelTitle, channel.Title);
                command.Parameters.AddWithValue("@" + ChannelSubTitle, channel.SubTitle);
                command.Parameters.AddWithValue("@" + ChannelLastUpdated, channel.LastUpdated);
                command.Parameters.Add("@" + ChannelThumbnail, DbType.Binary, channel.Thumbnail.Length).Value = channel.Thumbnail;
                command.Parameters.AddWithValue("@" + ChannelThumbnail, channel.Thumbnail);
                command.Parameters.AddWithValue("@" + ChannelSite, channel.Site);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Удалить канал
        /// </summary>
        /// <param name="parentID">ID канала</param>
        /// <returns></returns>
        public async Task DeleteChannelAsync(string parentID)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablechannels, ChannelId, parentID);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Переименовать канал
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <param name="newName">Новое название</param>
        /// <returns></returns>
        public async Task RenameChannelAsync(string id, string newName)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablechannels, ChannelTitle, newName,
                    ChannelId, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Записать канал со списком видео
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            await InsertChannelAsync(channel);
            foreach (IVideoItem item in channel.ChannelItems)
            {
                await InsertItemAsync(item);
            }
        }

        /// <summary>
        /// Получить видео
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        public async Task<VideoItemPOCO> GetVideoItemAsync(string id)
        {
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tableitems, ItemId, id);
            using (var command = GetCommand(zap))
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (!reader.HasRows)
                        throw new KeyNotFoundException("No item: " + id);

                    if (await reader.ReadAsync())
                    {
                        var vi = new VideoItemPOCO(reader);
                        return vi;
                    }
                }
            }
            throw new Exception(zap);
        }

        /// <summary>
        /// Получить список всех видео канала
        /// </summary>
        /// <param name="parentID">ID канала</param>
        /// <returns></returns>
        public async Task<List<VideoItemPOCO>> GetChannelItemsAsync(string parentID)
        {
            var res = new List<VideoItemPOCO>();
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tableitems, ParentID, parentID);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

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

        /// <summary>
        /// Записать видео
        /// </summary>
        /// <param name="item">Видео</param>
        /// <returns></returns>
        public async Task InsertItemAsync(IVideoItem item)
        {
            var zap =
                string.Format(
                    @"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')
                                    VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9})",
                    Tableitems,
                    ItemId,
                    ParentID,
                    Title,
                    Description,
                    ViewCount,
                    Duration,
                    Comments,
                    Thumbnail,
                    Timestamp
                    );
            using (var command = GetCommand(zap))
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

        /// <summary>
        /// Удалить видео
        /// </summary>
        /// <param name="id">ID видео</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(string id)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tableitems, ItemId, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Записать плэйлист
        /// </summary>
        /// <param name="playlist">Плэйлист</param>
        /// <returns></returns>
        public async Task InsertPlaylistAsync(IPlaylist playlist)
        {
            var zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}', '{5}') VALUES (@{1},@{2},@{3},@{4},@{5})",
                Tableplaylists,
                PlaylistID,
                PlaylistTitle,
                PlaylistSubTitle,
                PlaylistLink,
                PlaylistChannelId
                );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + PlaylistID, playlist.ID);
                command.Parameters.AddWithValue("@" + PlaylistTitle, playlist.Title);
                command.Parameters.AddWithValue("@" + PlaylistSubTitle, playlist.SubTitle);
                command.Parameters.AddWithValue("@" + PlaylistLink, playlist.Link);
                command.Parameters.AddWithValue("@" + PlaylistChannelId, playlist.ChannelId);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Удалить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        public async Task DeletePlaylistAsync(string id)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistID, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Получить плэйлист
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <returns></returns>
        public async Task<PlaylistPOCO> GetPlaylistAsync(string id)
        {
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistID, id);
            using (var command = GetCommand(zap))
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (!reader.HasRows)
                        throw new KeyNotFoundException("No item: " + id);

                    if (await reader.ReadAsync())
                    {
                        var pl = new PlaylistPOCO(reader);
                        return pl;
                    }
                }
            }

            throw new Exception(zap);
        }

        /// <summary>
        /// Обновить коллекцию видео, относящихся к плэйлисту
        /// </summary>
        /// <param name="playlistid">ID плэйлиста</param>
        /// <param name="itemid">ID видео</param>
        /// <param name="channelid">ID канала</param>
        /// <returns></returns>
        public async Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid)
        {
             var zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}','{3}') VALUES (@{1},@{2},@{3})",
                Tableplaylistitems,
                FPlaylistId,
                FItemId,
                FChannelId
                );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + FPlaylistId, playlistid);
                command.Parameters.AddWithValue("@" + FItemId, itemid);
                command.Parameters.AddWithValue("@" + FChannelId, channelid);

                await ExecuteNonQueryAsync(command);
            }

            zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}' AND {5}='{6}'", Tableplaylistitems,
                FPlaylistId, playlistid, FItemId, itemid, FChannelId, channelid);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Получить список видео, относящегося к плэйлисту канала
        /// </summary>
        /// <param name="id">ID плэйлиста</param>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<List<VideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID)
        {
            var res = new List<VideoItemPOCO>();
            var lst = new List<string>();
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}' AND {3}='{4}'", Tableplaylistitems, FPlaylistId, id, FChannelId, channelID);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

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

        /// <summary>
        /// Получить список всех плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<List<PlaylistPOCO>> GetChannelPlaylistAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistChannelId, channelID);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

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

        /// <summary>
        /// Записать тэг
        /// </summary>
        /// <param name="tag">Тэг</param>
        /// <returns></returns>
        public async Task InsertTagAsync(ITag tag)
        {
            var zap =
                string.Format(
                    @"INSERT INTO '{0}' ('{1}') VALUES (@{1})",
                    Tabletags,
                    TagTitle
                    );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + TagTitle, tag.Title);
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Получить тэг
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task DeleteTagAsync(string tag)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tabletags, TagTitle, tag);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Добавить тэг каналу
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task InsertChannelTagsAsync(string channelid, string tag)
        {
            var zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})",
              Tablechanneltags,
              ChannelIdF,
              TagIdF
              );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelIdF, channelid);
                command.Parameters.AddWithValue("@" + TagIdF, tag);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Удалить тэг у канала
        /// </summary>
        /// <param name="channelid">ID канала</param>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task DeleteChannelTagsAsync(string channelid, string tag)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}' AND {3}='{4}'", Tablechanneltags, ChannelIdF, channelid, TagIdF, tag);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Получить список каналов по тэгу
        /// </summary>
        /// <param name="tag">ID тэга</param>
        /// <returns></returns>
        public async Task<List<IChannelPOCO>> GetChannelsByTagAsync(string tag)
        {
            var res = new List<IChannelPOCO>();

            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablechanneltags, TagIdF, tag);

            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

                        while (await reader.ReadAsync())
                        {
                            var channel = await GetChannelAsync(reader[ChannelIdF].ToString());
                            res.Add(channel);
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Получить список тэгов канала
        /// </summary>
        /// <param name="id">ID канала</param>
        /// <returns></returns>
        public async Task<List<ITagPOCO>> GetChannelTagsAsync(string id)
        {
            var res = new List<ITagPOCO>();
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablechanneltags, ChannelIdF, id);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows)
                            return res;

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

        /// <summary>
        /// Получить credentials сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        public async Task<CredPOCO> GetCredAsync(string site)
        {
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablecredentials, CredSite, site);
            using (var command = GetCommand(zap))
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (!reader.HasRows)
                        throw new KeyNotFoundException("No item: " + site);

                    if (await reader.ReadAsync())
                    {
                        var cred = new CredPOCO(reader);
                        return cred;
                    }
                }
            }

            throw new Exception(zap);
        }

        /// <summary>
        /// Записать credential
        /// </summary>
        /// <param name="cred">Credential</param>
        /// <returns></returns>
        public async Task InsertCredAsync(ICred cred)
        {
            var zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6})",
               Tablecredentials,
               CredSite,
               CredLogin,
               CredPass,
               CredCookie,
               CredPasskey,
               CredAutorization
               );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + CredSite, cred.Site);
                command.Parameters.AddWithValue("@" + CredLogin, cred.Login);
                command.Parameters.AddWithValue("@" + CredPass, cred.Pass);
                command.Parameters.AddWithValue("@" + CredCookie, cred.Cookie);
                command.Parameters.AddWithValue("@" + CredPasskey, cred.Passkey);
                command.Parameters.AddWithValue("@" + CredAutorization, cred.Autorization);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Удалить credential
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <returns></returns>
        public async Task DeleteCredAsync(string site)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablecredentials, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Обновить логин к сайту
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newlogin">Новый логин</param>
        /// <returns></returns>
        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredLogin, newlogin, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Обновить пароль от сайта
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="newpassword">новый пароль</param>
        /// <returns></returns>
        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredPass, newpassword, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Обновить поле требовать авторизацию или нет
        /// </summary>
        /// <param name="site">ID сайта</param>
        /// <param name="autorize">0 - не требовать, 1 - требовать</param>
        /// <returns></returns>
        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredAutorization, autorize, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Записать настройку
        /// </summary>
        /// <param name="setting">Настройка</param>
        /// <returns></returns>
        public async Task InsertSettingAsync(ISetting setting)
        {
            var zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})",
             Tablesettings,
             SetKey,
             SetVal
             );
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + SetKey, setting.Key);
                command.Parameters.AddWithValue("@" + SetVal, setting.Value);

                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Удалить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        public async Task DeleteSettingAsync(string key)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablesettings, SetKey, key);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Обновить значение настройки
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <param name="newvalue">Новое значение</param>
        /// <returns></returns>
        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablesettings, SetVal, newvalue, SetKey, key);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// Получить настройку
        /// </summary>
        /// <param name="key">ID настройки</param>
        /// <returns></returns>
        public async Task<SettingPOCO> GetSettingAsync(string key)
        {
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}'", Tablesettings, SetKey, key);
            using (var command = GetCommand(zap))
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                await connection.OpenAsync();
                command.Connection = connection;
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (!reader.HasRows)
                        throw new KeyNotFoundException("No item: " + key);

                    if (await reader.ReadAsync())
                    {
                        var cred = new SettingPOCO(reader);
                        return cred;
                    }
                }
            }

            throw new Exception(zap);
        }

    }
}
