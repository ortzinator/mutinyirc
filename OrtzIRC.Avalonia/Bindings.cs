using Ninject.Modules;
using OrtzIRC.Avalonia.ViewModels;
using OrtzIRC.PluginFramework;

namespace OrtzIRC.Avalonia;

internal class Bindings : NinjectModule
{
    public override void Load()
    {
        Bind<MainViewModel>().ToSelf();
        Bind<ChannelViewModel>().ToSelf();
        Bind<PluginManager>().ToSelf().InSingletonScope();
    }
}
