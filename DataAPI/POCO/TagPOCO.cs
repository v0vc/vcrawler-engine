// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;

namespace DataAPI.POCO
{
    public class TagPOCO : IEquatable<TagPOCO>
    {
        #region Constructors

        public TagPOCO(string title)
        {
            Title = title;
        }

        #endregion

        #region Properties

        public string Title { get; private set; }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            return string.Format("{0}", Title).GetHashCode();
        }

        #endregion

        #region IEquatable<TagPOCO> Members

        public bool Equals(TagPOCO other)
        {
            return other.Title == Title;
        }

        #endregion
    }
}
