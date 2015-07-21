using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class TagPOCO : ITagPOCO
    {
        public TagPOCO(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
