// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using DataAPI.POCO;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class SubtitleFactory
    {
        #region ISubtitleFactory Members

        public ISubtitle CreateSubtitle()
        {
            return new Subtitle();
        }

        public ISubtitle CreateSubtitle(SubtitlePOCO poco)
        {
            return new Subtitle { Language = poco.Language, IsEnabled = true };
        }

        #endregion
    }
}
