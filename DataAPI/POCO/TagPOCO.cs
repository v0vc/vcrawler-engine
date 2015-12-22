// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved


namespace DataAPI.POCO
{
    public class TagPOCO
    {
        #region Constructors

        public TagPOCO(string title)
        {
            Title = title;
        }

        #endregion

        #region ITagPOCO Members

        public string Title { get; private set; }

        #endregion
    }
}
