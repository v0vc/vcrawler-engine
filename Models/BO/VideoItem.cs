using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class VideoItem :IVideoItem
    {
        private int _duration;
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ViewCount { get; set; }
        public int Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                DurationString = IntTostrTime(_duration);
            }
        }
        public string DurationString { get; set; }
        public int Comments { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }

        public VideoItem()
        {
            
        }
        public VideoItem(IVideoItemPOCO item)
        {
            ID = item.ID;
            Title = item.Title;
            ParentID = item.ParentID;
            Description = item.Description;
            ViewCount = item.ViewCount;
            Duration = item.Duration;
            Comments = item.Comments;
            Thumbnail = item.Thumbnail;
            Timestamp = item.Timestamp;
        }

        public IVideoItem CreateVideoItem()
        {
            return ServiceLocator.VideoItemFactory.CreateVideoItem();
        }

        public async Task<IVideoItem> GetVideoItemDbAsync()
        {
            return await ServiceLocator.VideoItemFactory.GetVideoItemDbAsync(ID);
        }

        public async Task<IVideoItem> GetVideoItemNetAsync()
        {
            return await ServiceLocator.VideoItemFactory.GetVideoItemNetAsync(ID);
        }

        public async Task<IChannel> GetParentChannelAsync()
        {
            return await ((VideoItemFactory) ServiceLocator.VideoItemFactory).GetParentChannelAsync(ParentID);
        }

        public async Task InsertItemAsync()
        {
            await ((VideoItemFactory) ServiceLocator.VideoItemFactory).InsertItemAsync(this);
        }

        public async Task DeleteItemAsync()
        {
            await ((VideoItemFactory) ServiceLocator.VideoItemFactory).DeleteItemAsync(ID);
        }

        public async Task UpdatePlaylistAsync(IPlaylist playlist)
        {
            await ((VideoItemFactory) ServiceLocator.VideoItemFactory).UpdatePlaylistAsync(playlist.ID, ID, ParentID);
        }

        internal string IntTostrTime(int duration)
        {
            TimeSpan t = TimeSpan.FromSeconds(duration);
            return t.Hours > 0 ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds) : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }
    }
}
