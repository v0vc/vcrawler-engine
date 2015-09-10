// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Tag : ITag
    {
        #region Static and Readonly Fields

        private readonly TagFactory _tf;

        #endregion

        #region Constructors

        public Tag(TagFactory tf)
        {
            _tf = tf;
        }

        private Tag()
        {
        }

        #endregion

        #region ITag Members

        public bool IsChecked { get; set; }
        public string Title { get; set; }

        public async Task DeleteTagAsync()
        {
            // return ServiceLocator.TagFactory.DeleteTagAsync(Title);
            await _tf.DeleteTagAsync(Title);
        }

        public async Task InsertTagAsync()
        {
            await _tf.InsertTagAsync(this);
        }

        #endregion
    }
}
