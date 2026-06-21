using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Covers the <see cref="ServerManager"/> singleton: its identity, the add/remove list
    ///   operations with their paired <c>ServerAdded</c>/<c>ServerRemoved</c> events, and the
    ///   connection-state aggregates (<see cref="ServerManager.AnyConnected"/> and
    ///   <see cref="ServerManager.DisconnectAll"/>).
    ///   <para>
    ///   <see cref="ServerManager.Instance"/> is a process-wide singleton with no reset hook, so
    ///   each test clears <c>ServerList</c> in setup and teardown to stay isolated, and every test
    ///   that subscribes to an event detaches its handler again so subscriptions don't leak onto the
    ///   shared instance.
    ///   </para>
    /// </summary>
    [TestFixture]
    public class ServerManagerTests
    {
        private ServerManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = ServerManager.Instance;
            _manager.ServerList.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _manager.ServerList.Clear();
        }

        [Test]
        public void Instance_ReturnsTheSameSingletonEachTime()
        {
            Assert.AreSame(ServerManager.Instance, ServerManager.Instance);
        }

        [Test]
        public void Create_AddsServerToListAndReturnsIt()
        {
            var server = _manager.Create(new ConnectionArgs("test", "irc.fake.com", false));

            Assert.IsNotNull(server);
            Assert.Contains(server, _manager.ServerList);
            Assert.AreEqual("irc.fake.com", server.Url);
        }

        [Test]
        public void Create_RaisesServerAddedWithTheCreatedServer()
        {
            Server raisedFor = null;
            void Handler(object sender, ServerEventArgs e) => raisedFor = e.Server;
            _manager.ServerAdded += Handler;

            try
            {
                var server = _manager.Create(new ConnectionArgs("test", "irc.fake.com", false));
                Assert.AreSame(server, raisedFor);
            }
            finally
            {
                _manager.ServerAdded -= Handler;
            }
        }

        [Test]
        public void Create_WithNoSubscribers_DoesNotThrow()
        {
            // ServerAdded.Fire must tolerate a null invocation list.
            Assert.DoesNotThrow(() => _manager.Create(new ConnectionArgs("test", "irc.fake.com", false)));
        }

        [Test]
        public void Remove_TakesServerOutOfList()
        {
            var server = _manager.Create(new ConnectionArgs("test", "irc.fake.com", false));

            _manager.Remove(server);

            Assert.IsFalse(_manager.ServerList.Contains(server));
        }

        [Test]
        public void Remove_RaisesServerRemovedWithTheServer()
        {
            var server = _manager.Create(new ConnectionArgs("test", "irc.fake.com", false));
            Server raisedFor = null;
            void Handler(object sender, ServerEventArgs e) => raisedFor = e.Server;
            _manager.ServerRemoved += Handler;

            try
            {
                _manager.Remove(server);
                Assert.AreSame(server, raisedFor);
            }
            finally
            {
                _manager.ServerRemoved -= Handler;
            }
        }

        [Test]
        public void Remove_ServerNotInList_DoesNotRaiseServerRemoved()
        {
            var stranger = ServerTestFactory.FakeServer(connected: false); // never added to the manager
            bool raised = false;
            void Handler(object sender, ServerEventArgs e) => raised = true;
            _manager.ServerRemoved += Handler;

            try
            {
                _manager.Remove(stranger);

                // The event is gated on List.Remove succeeding, so removing an unknown server is silent.
                Assert.IsFalse(raised);
            }
            finally
            {
                _manager.ServerRemoved -= Handler;
            }
        }

        [Test]
        public void AnyConnected_WithNoServers_ReturnsFalse()
        {
            Assert.IsFalse(_manager.AnyConnected());
        }

        [Test]
        public void AnyConnected_WithOnlyDisconnectedServers_ReturnsFalse()
        {
            _manager.ServerList.Add(ServerTestFactory.FakeServer(connected: false));
            _manager.ServerList.Add(ServerTestFactory.FakeServer(connected: false));

            Assert.IsFalse(_manager.AnyConnected());
        }

        [Test]
        public void AnyConnected_WithAtLeastOneConnectedServer_ReturnsTrue()
        {
            _manager.ServerList.Add(ServerTestFactory.FakeServer(connected: false));
            _manager.ServerList.Add(ServerTestFactory.FakeServer(connected: true));

            Assert.IsTrue(_manager.AnyConnected());
        }

        [Test]
        public void DisconnectAll_DisconnectsConnectedServersAndSkipsTheRest()
        {
            var connected = ServerTestFactory.FakeServer(connected: true);
            var disconnected = ServerTestFactory.FakeServer(connected: false);
            _manager.ServerList.Add(connected);
            _manager.ServerList.Add(disconnected);

            _manager.DisconnectAll();

            A.CallTo(() => connected.Connection.Disconnect(A<string>._)).MustHaveHappened();
            A.CallTo(() => disconnected.Connection.Disconnect(A<string>._)).MustNotHaveHappened();
        }
    }
}
