// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved
namespace Interfaces.Models
{
    public interface IChapter
    {
        #region Properties

        bool IsChecked { get; set; }
        bool IsEnabled { get; set; }
        string Language { get; set; }

        #endregion
    }
}
