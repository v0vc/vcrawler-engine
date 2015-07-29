using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface ITagFactory
    {
        ITag CreateTag();
        ITag CreateTag(ITagPOCO poco);
    }
}
