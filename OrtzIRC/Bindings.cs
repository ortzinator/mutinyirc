using Ninject.Modules;

namespace OrtzIRC
{
    internal class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<MainForm>().To(typeof(MainForm));
            Bind<ServerForm>().To(typeof(ServerForm));
        }
    }
}