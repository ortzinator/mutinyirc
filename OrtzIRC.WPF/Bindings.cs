using Ninject.Modules;
using OrtzIRC.PluginFramework;
using OrtzIRC.WPF.ViewModels;

namespace OrtzIRC.WPF
{
    internal class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<MainViewModel>().ToSelf();
            Bind<ChannelViewModel>().ToSelf();
            Bind<PluginManager>().ToSelf().InSingletonScope();
        }
    }
}