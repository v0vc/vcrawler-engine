// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.ObjectModel;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class AddTagViewModel
    {
        #region Constructors

        public AddTagViewModel()
        {
            Tags = new ObservableCollection<ITag>();
        }

        #endregion

        #region Properties

        public IChannel ParentChannel { get; set; }
        public ITag SelectedTag { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }

        #endregion
    }
}
