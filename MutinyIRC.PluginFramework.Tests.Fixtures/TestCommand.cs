namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using MutinyIRC.Common;
    using MutinyIRC.PluginFramework;

    [Plugin("test")]
    public class TestCommand : ICommand
    {
        // A parameterless Execute() that GetExecuteOverloads must skip without
        // throwing (its first-parameter filter would otherwise index an empty array).
        public CommandResultInfo Execute()
            => CommandResultInfo.Success("no-context");

        public CommandResultInfo Execute(TestMessageContext ctx)
            => CommandResultInfo.Success("noargs");

        public CommandResultInfo Execute(TestMessageContext ctx, string a)
            => CommandResultInfo.Success("string:" + a);

        public CommandResultInfo Execute(TestMessageContext ctx, string a, string b)
            => CommandResultInfo.Success("string,string:" + a + "|" + b);

        public CommandResultInfo Execute(TestMessageContext ctx, ChannelInfo channel, string msg)
            => CommandResultInfo.Success("channel,string:" + channel.Name + "|" + msg);

        public CommandResultInfo Execute(TestMessageContext ctx, char[] flags)
            => CommandResultInfo.Success("switch:" + new string(flags));

        public CommandResultInfo Execute(OtherTestMessageContext ctx, string a)
            => CommandResultInfo.Success("other-context:" + a);
    }
}
