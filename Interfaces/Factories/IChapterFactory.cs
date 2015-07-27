using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IChapterFactory
    {
        IChapter CreateChapter();
        IChapter CreateChapter(IChapterPOCO poco);
    }
}
