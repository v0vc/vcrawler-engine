using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class TagPOCO :ITagPOCO
    {
        public string Title { get; set; }

        public TagPOCO(string title)
        {
            Title = title;
        }
    }
}
