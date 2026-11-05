using System;

namespace MutinyIRC.PluginFramework.Tests.Fixtures;

[Plugin("throws")]
public class ThrowingCommand : ICommand
{
    public CommandResultInfo Execute(TestMessageContext ctx)
    {
        throw new InvalidOperationException("boom");
    }
}
