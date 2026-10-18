using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.PluginFramework.Tests.Fixtures;

[Plugin("test")]
public class TestCommand : ICommand
{
    // A parameterless Execute() that GetExecuteOverloads must skip without
    // throwing (its first-parameter filter would otherwise index an empty array).
    public CommandResultInfo Execute()
        => CommandResultInfo.Success("no-context");

    public CommandResultInfo Execute(TestMessageContext _)
        => CommandResultInfo.Success("noargs");

    public CommandResultInfo Execute(TestMessageContext _, string a)
        => CommandResultInfo.Success("string:" + a);

    public CommandResultInfo Execute(TestMessageContext _, string a, string b)
        => CommandResultInfo.Success("string,string:" + a + "|" + b);

    public CommandResultInfo Execute(TestMessageContext _, ChannelInfo channel, string msg)
        => CommandResultInfo.Success("channel,string:" + channel.Name + "|" + msg);

    public CommandResultInfo Execute(TestMessageContext _, char[] flags)
        => CommandResultInfo.Success("switch:" + new string(flags));

    public CommandResultInfo Execute(OtherTestMessageContext _, string a)
        => CommandResultInfo.Success("other-context:" + a);
}
