// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;

namespace Models.BO.Channels
{
    public class CommonChannel
    {
        #region Static Properties

        protected static Dictionary<string, string> Stats
            =>
                new Dictionary<string, string>
                {
                    { "Like", "LikeCount" },
                    { "Dis", "DislikeCount" },
                    { "Comm", "Comments" },
                    { "Diff", "ViewDiff" },
                };

        public IEnumerable<string> Statistics => new List<string> { "Like", "Dis", "Comm", "Diff" };
        #endregion
    }
}
