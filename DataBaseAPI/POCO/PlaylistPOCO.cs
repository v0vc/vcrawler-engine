using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    [DataContract]
    public class PlaylistPOCO :IPlaylistPOCO
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string SubTitle { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string ChannelID { get; set; }

        public PlaylistPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.PlaylistID] as string;
            Title = reader[SqLiteDatabase.PlaylistTitle] as string;
            SubTitle = reader[SqLiteDatabase.PlaylistSubTitle] as string;
            Link = reader[SqLiteDatabase.PlaylistLink] as string;
            ChannelID = reader[SqLiteDatabase.PlaylistChannelId] as string;
        }
        
    }

    //public static class StringExt
    //{
    //    public static string Truncate(this string value, int maxLength)
    //    {
    //        if (string.IsNullOrEmpty(value)) return value;
    //        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    //    }
    //}
}
