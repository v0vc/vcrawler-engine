using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class ChapterFactory : IChapterFactory
    {
        public IChapter CreateChapter()
        {
            return new Chapter();
        }

        public IChapter CreateChapter(IChapterPOCO poco)
        {
            var chapter = new Chapter
            {
                Language = poco.Language, 
                IsEnabled = true
            };
            return chapter;
        }
    }
}
