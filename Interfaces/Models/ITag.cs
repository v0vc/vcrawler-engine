using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ITag
    {
        bool IsChecked { get; set; }
        string Title { get; set; }
        Task DeleteTagAsync();
        Task InsertTagAsync();
    }
}
