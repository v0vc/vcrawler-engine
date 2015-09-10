// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class ChapterFactory : IChapterFactory
    {
        #region IChapterFactory Members

        public IChapter CreateChapter()
        {
            return new Chapter();
        }

        public IChapter CreateChapter(IChapterPOCO poco)
        {
            var chapter = new Chapter { Language = poco.Language, IsEnabled = true };
            return chapter;
        }

        #endregion
    }
}
