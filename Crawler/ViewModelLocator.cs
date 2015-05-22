using Crawler.Common;
using Crawler.ViewModels;
using Ninject;

namespace Crawler
{
    public class ViewModelLocator
    {
        public static MainWindowViewModel MvViewModel
        {
            get { return NinjectContainer.VmKernel.Get<MainWindowViewModel>(); }
        }
    }
}
