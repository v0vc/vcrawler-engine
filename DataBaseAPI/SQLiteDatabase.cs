using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Reflection;
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
        private const string Dbfile = "db.sqlite";
        private const string SqlSchemaFolder = "Schema";
        private const string SqlFile = "sqlite.sql";
        private readonly string _appstartdir;
        private readonly string _dbConnection;

        public SqLiteDatabase()
        {
            _appstartdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_appstartdir == null)
            {
                return;
            }
            var fdb = Path.Combine(_appstartdir, Dbfile);
            FileBase = new FileInfo(fdb);
            _dbConnection = string.Format(
                    "Data Source={0};Version=3;foreign keys=true;Count Changes=off;Journal Mode=off;Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off", 
                    FileBase.FullName);
            if (!FileBase.Exists)
            {
                CreateDb();
            }
        }

        public FileInfo FileBase { get; set; }

        public async Task<IChannelPOCO> GetChannelAsync(string id)
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

            throw new Exception(zap);
        }

        public async Task<List<IChannelPOCO>> GetChannelsListAsync()
        {
            var res = new List<IChannelPOCO>();

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

        public async Task InsertChannelAsync(IChannel channel)
        {
            var zap =
                string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}') VALUES (@{1},@{2},@{3},@{4},@{5})",
                    Tablechannels,
                    ChannelId,
                    ChannelTitle,
                    ChannelSubTitle,
                    ChannelThumbnail,
                    ChannelSite);

            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelId, channel.ID);
                command.Parameters.AddWithValue("@" + ChannelTitle, channel.Title);
                command.Parameters.AddWithValue("@" + ChannelSubTitle, channel.SubTitle);
                command.Parameters.Add("@" + ChannelThumbnail, DbType.Binary, channel.Thumbnail.Length).Value = channel.Thumbnail;
                command.Parameters.AddWithValue("@" + ChannelThumbnail, channel.Thumbnail);
                command.Parameters.AddWithValue("@" + ChannelSite, channel.Site);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task DeleteChannelAsync(string parentID)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablechannels, ChannelId, parentID);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task RenameChannelAsync(string id, string newName)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablechannels, ChannelTitle, newName, ChannelId, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            await InsertChannelAsync(channel);

            foreach (var item in channel.ChannelItems)
            {
                await InsertItemAsync(item);
            }
        }

        public async Task<IVideoItemPOCO> GetVideoItemAsync(string id)
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
                    {
                        throw new KeyNotFoundException("No item: " + id);
                    }

                    if (await reader.ReadAsync())
                    {
                        var vi = new VideoItemPOCO(reader);
                        return vi;
                    }
                }
            }
            throw new Exception(zap);
        }

        public async Task<List<IVideoItemPOCO>> GetChannelItemsAsync(string parentID)
        {
            var res = new List<IVideoItemPOCO>();
            var zap = string.Format("SELECT * FROM {0} WHERE {1}='{2}' ORDER BY {3} DESC", Tableitems, ParentID, parentID, Timestamp);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
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
                    Timestamp);

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

        public async Task DeleteItemAsync(string id)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tableitems, ItemId, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertPlaylistAsync(IPlaylist playlist)
        {
            var zap =
                string.Format(@"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}', '{5}') VALUES (@{1},@{2},@{3},@{4},@{5})",
                    Tableplaylists,
                    PlaylistID,
                    PlaylistTitle,
                    PlaylistSubTitle,
                    PlaylistThumbnail,
                    PlaylistChannelId);

            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + PlaylistID, playlist.ID);
                command.Parameters.AddWithValue("@" + PlaylistTitle, playlist.Title);
                command.Parameters.AddWithValue("@" + PlaylistSubTitle, playlist.SubTitle);
                command.Parameters.Add("@" + PlaylistThumbnail, DbType.Binary, playlist.Thumbnail.Length).Value = playlist.Thumbnail;
                command.Parameters.AddWithValue("@" + PlaylistChannelId, playlist.ChannelId);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task DeletePlaylistAsync(string id)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tableplaylists, PlaylistID, id);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task<IPlaylistPOCO> GetPlaylistAsync(string id)
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

            throw new Exception(zap);
        }

        public async Task UpdatePlaylistAsync(string playlistid, string itemid, string channelid)
        {
            // OR IGNORE
            var zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}','{3}') VALUES (@{1},@{2},@{3})", 
                Tableplaylistitems, 
                FPlaylistId, 
                FItemId, 
                FChannelId);

            using (var command = GetCommand(zap))
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

        public async Task<List<IVideoItemPOCO>> GetPlaylistItemsAsync(string id, string channelID)
        {
            var res = new List<IVideoItemPOCO>();
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

            foreach (var itemid in lst)
            {
                res.Add(await GetVideoItemAsync(itemid));
            }

            return res;
        }

        public async Task<List<IPlaylistPOCO>> GetChannelPlaylistAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();
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

        public async Task InsertTagAsync(ITag tag)
        {
            var zap =
                string.Format(
                    @"INSERT INTO '{0}' ('{1}') VALUES (@{1})", 
                    Tabletags, 
                    TagTitle);

            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + TagTitle, tag.Title);
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task DeleteTagAsync(string tag)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tabletags, TagTitle, tag);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertChannelTagsAsync(string channelid, string tag)
        {
            var zap = string.Format(@"INSERT OR IGNORE INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", 
                Tablechanneltags, 
                ChannelIdF, 
                TagIdF);

            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + ChannelIdF, channelid);
                command.Parameters.AddWithValue("@" + TagIdF, tag);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task DeleteChannelTagsAsync(string channelid, string tag)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}' AND {3}='{4}'", Tablechanneltags, ChannelIdF, channelid, TagIdF, tag);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

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
                        {
                            return res;
                        }

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

        public async Task<List<ICredPOCO>> GetCredListAsync()
        {
            var res = new List<ICredPOCO>();
            var zap = string.Format("SELECT * FROM {0}", Tablecredentials);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
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

        public async Task<ICredPOCO> GetCredAsync(string site)
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

            throw new Exception(zap);
        }

        public async Task InsertCredAsync(ICred cred)
        {
            var zap =
                string.Format(
                    @"INSERT INTO '{0}' ('{1}','{2}','{3}','{4}','{5}','{6}') VALUES (@{1},@{2},@{3},@{4},@{5},@{6})", 
                    Tablecredentials, 
                    CredSite, 
                    CredLogin, 
                    CredPass, 
                    CredCookie, 
                    CredExpired, 
                    CredAutorization);

            using (var command = GetCommand(zap))
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

        public async Task DeleteCredAsync(string site)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablecredentials, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredLogin, newlogin, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredPass, newpassword, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdateCookieAsync(string site, string newcookie)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredCookie, newcookie, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdateExpiredAsync(string site, DateTime newexpired)
        {
            var zap = string.Format("UPDATE {0} SET {1}=@newexpired WHERE {2}='{3}'", Tablecredentials, CredExpired, CredSite, site);
            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@newexpired", newexpired);
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablecredentials, CredAutorization, autorize, CredSite, site);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task InsertSettingAsync(ISetting setting)
        {
            var zap = string.Format(@"INSERT INTO '{0}' ('{1}','{2}') VALUES (@{1},@{2})", 
                Tablesettings, 
                SetKey, 
                SetVal);

            using (var command = GetCommand(zap))
            {
                command.Parameters.AddWithValue("@" + SetKey, setting.Key);
                command.Parameters.AddWithValue("@" + SetVal, setting.Value);

                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task DeleteSettingAsync(string key)
        {
            var zap = string.Format("DELETE FROM {0} WHERE {1}='{2}'", Tablesettings, SetKey, key);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            var zap = string.Format("UPDATE {0} SET {1}='{2}' WHERE {3}='{4}'", Tablesettings, SetVal, newvalue, SetKey, key);
            using (var command = GetCommand(zap))
            {
                await ExecuteNonQueryAsync(command);
            }
        }

        public async Task<ISettingPOCO> GetSettingAsync(string key)
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

            throw new Exception(zap);
        }

        public async Task<int> GetChannelItemsCountDbAsync(string channelID)
        {
            var zap = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}='{2}'", Tableitems, ParentID, channelID);
            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    var res = await command.ExecuteScalarAsync(CancellationToken.None);

                    if (res == null || res == DBNull.Value)
                    {
                        throw new Exception(zap);
                    }

                    return Convert.ToInt32(res);
                }
            }
        }

        public async Task<List<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            var res = new List<string>();

            var zap = string.Format("SELECT {0} FROM {1} WHERE {2}='{3}'", FItemId, Tableplaylistitems, FPlaylistId, id);

            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
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

        public async Task<List<string>> GetChannelsIdsListDbAsync()
        {
            var res = new List<string>();

            var zap = string.Format("SELECT {0} FROM {1}", ChannelId, Tablechannels);

            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
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

        public async Task<List<string>> GetChannelItemsIdListDbAsync(string channelID)
        {
            var res = new List<string>();

            var zap = string.Format("SELECT {0} FROM {1} WHERE {2}='{3}' ORDER BY {4} DESC", ItemId, Tableitems, ParentID, channelID, Timestamp);

            using (var command = GetCommand(zap))
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
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

        public async Task VacuumAsync()
        {
            using (var command = GetCommand("vacuum"))
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

        private static SQLiteCommand GetCommand(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("sql");
            }

            return new SQLiteCommand {CommandText = sql, CommandType = CommandType.Text};
        }

        private async void CreateDb()
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
            {
                throw new FileNotFoundException("SQL Scheme not found in " + fnsch.FullName);
            }
        }

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

        public static readonly string ChannelThumbnail = "thumbnail";

        public static readonly string ChannelSite = "site";

        #endregion

        #region channeltags

        public static readonly string TagIdF = "tagid";

        public static readonly string ChannelIdF = "channelid";

        #endregion

        #region playlists

        public static readonly string PlaylistID = "id";

        public static readonly string PlaylistTitle = "title";

        public static readonly string PlaylistSubTitle = "subtitle";

        public static readonly string PlaylistThumbnail = "thumbnail";

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

        public static readonly string CredExpired = "expired";

        public static readonly string CredAutorization = "autorization";

        #endregion

        #region settings

        public static readonly string SetKey = "key";

        public static readonly string SetVal = "val";

        #endregion

        #region tags

        public static readonly string TagTitle = "title";

        #endregion
    }
}
