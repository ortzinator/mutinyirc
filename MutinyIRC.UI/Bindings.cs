using Ninject.Modules;
using MutinyIRC.UI.ViewModels;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.UI;

internal class Bindings : NinjectModule
{
    public override void Load()
    {
        Bind<MainViewModel>().ToSelf();
        Bind<ChannelViewModel>().ToSelf();
        Bind<PrivateMessageViewModel>().ToSelf();
        Bind<PluginManager>().ToSelf().InSingletonScope();
    }
}
