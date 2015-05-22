using Ninject;

namespace Crawler.Common
{
    public class NinjectContainer
    {
        private static IKernel _vmKernel;

        public static IKernel VmKernel
        {
            get
            {
                if (_vmKernel == null)
                {
                    _vmKernel = new StandardKernel(new NinjectModuleViewModels());
                }
                return _vmKernel;
            }
        }
    }
}
