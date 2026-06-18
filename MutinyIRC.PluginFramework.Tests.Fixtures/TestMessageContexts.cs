namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using MutinyIRC.Common;

    // Test doubles not tied to a real connection; OwningServer is unused by these tests.
    public class TestMessageContext : MessageContext
    {
        public override Server OwningServer => null;
    }

    public class OtherTestMessageContext : MessageContext
    {
        public override Server OwningServer => null;
    }
}
