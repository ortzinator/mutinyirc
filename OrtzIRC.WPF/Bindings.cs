using Ninject.Modules;
using OrtzIRC.PluginFramework;
using OrtzIRC.WPF.ViewModels;

namespace OrtzIRC.WPF
{
    internal class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<MainViewModel>().To(typeof(MainViewModel));
            Bind<ChannelViewModel>().To(typeof(ChannelViewModel));
            Bind<PluginManager>().To(typeof(PluginManager));
        }
    }
}