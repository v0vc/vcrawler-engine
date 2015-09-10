// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class TagFactory : ITagFactory
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _c;

        #endregion

        #region Constructors

        public TagFactory(ICommonFactory c)
        {
            _c = c;
        }

        #endregion

        #region Methods

        public async Task DeleteTagAsync(string tag)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();

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
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
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

        public ITag CreateTag(ITagPOCO poco)
        {
            var tag = new Tag(this) { Title = poco.Title };
            return tag;
        }

        #endregion
    }
}
