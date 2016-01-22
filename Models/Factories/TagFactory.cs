// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using DataAPI.POCO;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public static class TagFactory
    {
        #region Static Methods

        public static ITag CreateTag()
        {
            return new Tag();
        }

        public static ITag CreateTag(TagPOCO poco)
        {
            var tag = new Tag { Title = poco.Title };
            return tag;
        }

        #endregion
    }
}
