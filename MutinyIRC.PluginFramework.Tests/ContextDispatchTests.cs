namespace MutinyIRC.PluginFramework.Tests
{
    using System.Collections.Generic;
    using FlamingIRC;
    using MutinyIRC.Common;
    using MutinyIRC.PluginFramework;
    using MutinyIRC.PluginFramework.Tests.Fixtures;
    using NUnit.Framework;

    /// <summary>
    /// Covers the dispatcher generalization: a single <c>Execute(MessageContext, …)</c> overload
    /// matches any context (assignability), while a more-derived overload still wins at equal
    /// arity (specificity ordering), and a <see cref="Server"/> subclass matches a
    /// <see cref="Server"/>-typed parameter.
    /// </summary>
    [TestFixture]
    public class ContextDispatchTests
    {
        private PluginManager _manager;
        private Server _server;
        private Channel _channel;
        private PrivateMessageSession _pm;

        [SetUp]
        public void SetUp()
        {
            _manager = new PluginManager();
            RegisterFixture<ContextGeneralizationCommand>("general");
            RegisterFixture<ContextSpecificityCommand>("specificity");
            RegisterFixture<ServerOnlyCommand>("serveronly");

            _server = new Server();
            _channel = new Channel(_server, "#mutiny");
            _pm = new PrivateMessageSession(_server, new User { Nick = "someone" });
        }

        private void RegisterFixture<T>(string name) where T : ICommand
        {
            var type = typeof(T);
            _manager._commands.Add(type.FullName,
                new CommandInfo(type.Assembly.Location, type.FullName, name, typeof(ICommand)));
        }

        private CommandResultInfo Dispatch(string name, MessageContext context, params object[] args)
            => _manager.ExecuteCommand(new CommandExecutionInfo
            {
                Name = name,
                Context = context,
                ParameterList = new List<object>(args),
            });

        [Test]
        public void SingleMessageContextOverload_MatchesChannelContext()
        {
            var result = Dispatch("general", _channel, "hi");

            Assert.That(result.Message, Is.EqualTo("context:Channel:hi"));
        }

        [Test]
        public void SingleMessageContextOverload_MatchesServerContext()
        {
            var result = Dispatch("general", _server, "hi");

            Assert.That(result.Message, Is.EqualTo("context:Server:hi"));
        }

        [Test]
        public void SingleMessageContextOverload_MatchesPrivateMessageContext()
        {
            var result = Dispatch("general", _pm, "hi");

            Assert.That(result.Message, Is.EqualTo("context:PrivateMessageSession:hi"));
        }

        [Test]
        public void EqualArity_ChannelOverloadWinsFromChannelContext()
        {
            var result = Dispatch("specificity", _channel, "hi");

            Assert.That(result.Message, Is.EqualTo("channel:hi"));
        }

        [Test]
        public void EqualArity_FallsBackToMessageContextOverloadFromOtherContexts()
        {
            Assert.That(Dispatch("specificity", _server, "hi").Message, Is.EqualTo("general:hi"));
            Assert.That(Dispatch("specificity", _pm, "hi").Message, Is.EqualTo("general:hi"));
        }

        [Test]
        public void ServerTypedOverload_MatchesServerSubclass()
        {
            // The old exact GetType() check rejected proxy/subclass instances; assignability accepts them.
            var result = Dispatch("serveronly", new DerivedServer(), "hi");

            Assert.That(result.Message, Is.EqualTo("server:hi"));
        }

        [Test]
        public void ServerTypedOverload_RejectsUnrelatedContext()
        {
            // A Server-only command must not match a Channel/PM context.
            Assert.That(Dispatch("serveronly", _channel, "hi"), Is.Null);
        }
    }
}
