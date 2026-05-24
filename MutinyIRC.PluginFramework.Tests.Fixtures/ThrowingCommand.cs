namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using System;
    using MutinyIRC.PluginFramework;

    [Plugin("throws")]
    public class ThrowingCommand : ICommand
    {
        public CommandResultInfo Execute(TestMessageContext ctx)
        {
            throw new InvalidOperationException("boom");
        }
    }
}
