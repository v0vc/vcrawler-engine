// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using DataAPI.POCO;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class TagFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;

        #endregion

        #region Constructors

        public TagFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
        }

        #endregion

        #region Methods

        public async Task DeleteTagAsync(string tag)
        {
            var fb = commonFactory.CreateSqLiteDatabase();

            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertTagAsync(ITag tag)
        {
            var fb = commonFactory.CreateSqLiteDatabase();
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

        #region ITagFactory Members

        public ITag CreateTag()
        {
            return new Tag(this);
        }

        public ITag CreateTag(TagPOCO poco)
        {
            var tag = new Tag(this) { Title = poco.Title };
            return tag;
        }

        #endregion
    }
}
