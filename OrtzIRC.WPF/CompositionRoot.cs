using Ninject;
using Ninject.Modules;
using Ninject.Parameters;

namespace OrtzIRC.WPF
{
    public class CompositionRoot
    {
        private static IKernel _ninjectKernel;

        public static void Wire(INinjectModule module)
        {
            _ninjectKernel = new StandardKernel(module);
        }

        public static T Resolve<T>()
        {
            return _ninjectKernel.Get<T>();
        }

        public static T Resolve<T>(params IParameter[] parameterObjects)
        {
            return _ninjectKernel.Get<T>(parameterObjects);
        }
    }
}