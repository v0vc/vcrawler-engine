using Interfaces.Models;

namespace Models.BO
{
    public class Chapter : IChapter
    {
        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; }
        public string Language { get; set; }
    }
}
