namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using MutinyIRC.Common;
    using MutinyIRC.PluginFramework;

    /// <summary>
    /// Declares a single <c>Execute(MessageContext, …)</c> overload — the case the dispatcher
    /// generalization is meant to enable. It must match whatever concrete context it is given.
    /// </summary>
    [Plugin("general")]
    public class ContextGeneralizationCommand : ICommand
    {
        public CommandResultInfo Execute(MessageContext context, string a)
            => CommandResultInfo.Success("context:" + context.GetType().Name + ":" + a);
    }

    /// <summary>
    /// Declares both a general and a <see cref="Channel"/>-specific overload of equal arity, to
    /// prove the specificity tiebreak: a channel context picks the channel overload while any
    /// other context falls through to the general one.
    /// </summary>
    [Plugin("specificity")]
    public class ContextSpecificityCommand : ICommand
    {
        public CommandResultInfo Execute(MessageContext context, string a)
            => CommandResultInfo.Success("general:" + a);

        public CommandResultInfo Execute(Channel channel, string a)
            => CommandResultInfo.Success("channel:" + a);
    }

    /// <summary>
    /// Declares only a <see cref="Server"/>-typed overload, used to prove that a *subclass* of
    /// <see cref="Server"/> (e.g. a mocking-framework proxy) now matches where the old exact
    /// <c>GetType()</c> check would have rejected it.
    /// </summary>
    [Plugin("serveronly")]
    public class ServerOnlyCommand : ICommand
    {
        public CommandResultInfo Execute(Server server, string a)
            => CommandResultInfo.Success("server:" + a);
    }

    /// <summary>A trivial <see cref="Server"/> subclass standing in for a mock proxy type.</summary>
    public class DerivedServer : Server { }
}
