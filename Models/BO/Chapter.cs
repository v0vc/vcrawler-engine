// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Interfaces.Models;

namespace Models.BO
{
    public class Chapter : IChapter
    {
        #region IChapter Members

        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; }
        public string Language { get; set; }

        #endregion
    }
}
