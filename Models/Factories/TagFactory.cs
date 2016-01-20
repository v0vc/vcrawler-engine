// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using DataAPI.Database;
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

        public static async Task DeleteTagAsync(string tag)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();

            try
            {
                await fb.DeleteTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task InsertTagAsync(ITag tag)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.InsertTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
