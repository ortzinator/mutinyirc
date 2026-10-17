using FlamingIRC;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework.Tests.Fixtures;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.PluginFramework.Tests;

/// <summary>
/// Covers the dispatcher generalization: a single <c>Execute(MessageContext, …)</c> overload
/// matches any context (assignability), while a more-derived overload still wins at equal
/// arity (specificity ordering), and a <see cref="Server"/> subclass matches a
/// <see cref="Server"/>-typed parameter.
/// </summary>
[TestFixture]
public class ContextDispatchTests
{
    private CommandHarness _harness;
    private Server _server;
    private Channel _channel;
    private PrivateMessageSession _pm;

    [SetUp]
    public void SetUp()
    {
        _harness = new CommandHarness()
            .Register<ContextGeneralizationCommand>("general")
            .Register<ContextSpecificityCommand>("specificity")
            .Register<ServerOnlyCommand>("serveronly");

        _server = new Server();
        _channel = new Channel(_server, "#mutiny");
        _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });
    }

    [Test]
    public void SingleMessageContextOverload_MatchesChannelContext()
    {
        var result = _harness.Dispatch("general", _channel, "hi");

        Assert.That(result.Message, Is.EqualTo("context:Channel:hi"));
    }

    [Test]
    public void SingleMessageContextOverload_MatchesServerContext()
    {
        var result = _harness.Dispatch("general", _server, "hi");

        Assert.That(result.Message, Is.EqualTo("context:Server:hi"));
    }

    [Test]
    public void SingleMessageContextOverload_MatchesPrivateMessageContext()
    {
        var result = _harness.Dispatch("general", _pm, "hi");

        Assert.That(result.Message, Is.EqualTo("context:PrivateMessageSession:hi"));
    }

    [Test]
    public void EqualArity_ChannelOverloadWinsFromChannelContext()
    {
        var result = _harness.Dispatch("specificity", _channel, "hi");

        Assert.That(result.Message, Is.EqualTo("channel:hi"));
    }

    [Test]
    public void EqualArity_FallsBackToMessageContextOverloadFromOtherContexts()
    {
        var fromServer = _harness.Dispatch("specificity", _server, "hi");
        var fromPm = _harness.Dispatch("specificity", _pm, "hi");

        Assert.That(fromServer.Message, Is.EqualTo("general:hi"));
        Assert.That(fromPm.Message, Is.EqualTo("general:hi"));
    }

    [Test]
    public void ServerTypedOverload_MatchesServerSubclass()
    {
        // The old exact GetType() check rejected proxy/subclass instances; assignability accepts them.
        var result = _harness.Dispatch("serveronly", new DerivedServer(), "hi");

        Assert.That(result.Message, Is.EqualTo("server:hi"));
    }

    [Test]
    public void ServerTypedOverload_RejectsUnrelatedContext()
    {
        // A Server-only command must not match a Channel/PM context.
        Assert.That(_harness.Dispatch("serveronly", _channel, "hi"), Is.Null);
    }
}
