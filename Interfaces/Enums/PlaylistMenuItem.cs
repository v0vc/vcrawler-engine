// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;

namespace Interfaces.Enums
{
    public enum PlaylistMenuItem
    {
        Link,
        [Description("720p")]
        Download,
        [Description("HD")]
        DownloadHd,
        [Description("Audio only")]
        Audio,
        [Description("Video only")]
        Video,
        Update
    }
}
