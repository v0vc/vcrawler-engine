namespace Interfaces.Models
{
    public interface IChapter
    {
        bool IsChecked { get; set; }

        bool IsEnabled { get; set; }

        string Language { get; set; }
    }
}
