using Ninject;
using Ninject.Modules;
using Ninject.Parameters;

namespace MutinyIRC.UI;

public class CompositionRoot
{
    private static IKernel NinjectKernel = null!;

    public static void Wire(INinjectModule module)
    {
        NinjectKernel = new StandardKernel(module);
    }

    public static T Resolve<T>()
    {
        return NinjectKernel.Get<T>();
    }

    public static T Resolve<T>(params IParameter[] parameterObjects)
    {
        return NinjectKernel.Get<T>(parameterObjects);
    }
}
