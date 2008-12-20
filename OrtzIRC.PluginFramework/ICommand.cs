namespace OrtzIRC.PluginFramework
{
    using OrtzIRC.Common;

    public interface ICommand : IPlugin
    {
        void Execute(Channel channel, params string[] parameters);
        void Execute(Server server, params string[] parameters);
        void Execute(PrivateMessageSession pmSession, params string[] parameters);
    }
}
