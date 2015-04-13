using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace Models.IoC
{
    class Container
    {
        private static IKernel _kernel;

        public static IKernel Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    _kernel = GetKernel();
                }
                return _kernel;
            }
        }

        private static IKernel GetKernel()
        {
            var module = new Module();
            INinjectModule[] modules = { module };
            return new StandardKernel(modules);
        }
    }
}
