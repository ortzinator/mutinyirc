using System.IO;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Covers <see cref="TextLoggerManager"/>'s on/off lifecycle: enabling registers the servers
    ///   already known to <see cref="ServerManager"/>; while active, the live <c>ServerAdded</c>
    ///   subscription registers servers added later; toggling is idempotent and does not
    ///   double-subscribe; and the per-call <c>LoggerActive</c> guard short-circuits writes while
    ///   disabled. The timestamp/format passthroughs to <see cref="TextLogger"/> are pinned too.
    ///   <para>
    ///   These exercise a lot of process-wide static state — <see cref="TextLoggerManager"/> itself,
    ///   <see cref="TextLogger"/>'s file table, and the <see cref="ServerManager"/> singleton — so
    ///   setup/teardown reset all of it and redirect <see cref="LoggedItem.basedir"/> at a throwaway
    ///   temp directory, keeping real log files out of the picture. Teardown disables the logger
    ///   first so every open file handle is closed before the temp directory is deleted.
    ///   </para>
    /// </summary>
    [TestFixture]
    public class TextLoggerManagerTests
    {
        private string _originalBaseDir;
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            TextLoggerManager.LoggerActive = false;
            ServerManager.Instance.ServerList.Clear();

            _originalBaseDir = LoggedItem.basedir;
            _tempDir = Path.Combine(Path.GetTempPath(), "MutinyIRC.Tests", Path.GetRandomFileName());
            LoggedItem.basedir = _tempDir;
        }

        [TearDown]
        public void TearDown()
        {
            // Disabling runs TurnOff -> RemoveAllLoggables, which closes every open FileStream and
            // unsubscribes the manager, so the temp directory is no longer locked.
            TextLoggerManager.LoggerActive = false;
            ServerManager.Instance.ServerList.Clear();
            LoggedItem.basedir = _originalBaseDir;

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // A Server backed by a fake connection so it carries a known Url (the log-file key) without
        // opening a socket.
        private static Server FakeServer(string host)
        {
            var conn = A.Fake<IConnection>();
            A.CallTo(() => conn.ConnectionData).Returns(new ConnectionArgs("nick", host, false));
            return new Server { Connection = conn };
        }

        private static string ServerLogPath(string tempDir, string host)
            => Path.Combine(tempDir, host, "!" + host + ".log");

        [Test]
        public void EnablingLogger_RegistersServersAlreadyInTheManager()
        {
            var server = FakeServer("a.example");
            ServerManager.Instance.ServerList.Add(server);

            TextLoggerManager.LoggerActive = true;

            // A registered server has a log file; TextEntry resolves it instead of throwing KeyNotFound.
            Assert.DoesNotThrow(() => TextLoggerManager.TextEntry(server, "hello"));
            Assert.IsTrue(File.Exists(ServerLogPath(_tempDir, "a.example")));
        }

        [Test]
        public void WhenActive_ServerAddedThroughManager_IsRegisteredForLogging()
        {
            TextLoggerManager.LoggerActive = true; // subscribes before any server exists

            var server = ServerManager.Instance.Create(new ConnectionArgs("test", "irc.fake.com", false));

            // The live ServerAdded subscription must have registered the newly created server.
            Assert.DoesNotThrow(() => TextLoggerManager.TextEntry(server, "hi"));
            Assert.IsTrue(File.Exists(ServerLogPath(_tempDir, "irc.fake.com")));
        }

        [Test]
        public void EnablingLoggerTwice_DoesNotReRegisterExistingServers()
        {
            var server = FakeServer("a.example");
            ServerManager.Instance.ServerList.Add(server);

            TextLoggerManager.LoggerActive = true; // registers a.example once

            // The second assignment is a no-op; if it re-ran TurnOn it would AddLoggable a second
            // time and throw a duplicate-key ArgumentException.
            Assert.DoesNotThrow(() => TextLoggerManager.LoggerActive = true);
        }

        [Test]
        public void TogglingLogger_DoesNotDoubleSubscribeToServerAdded()
        {
            TextLoggerManager.LoggerActive = true;
            TextLoggerManager.LoggerActive = false;
            TextLoggerManager.LoggerActive = true;

            // If TurnOff had failed to detach the ServerAdded handler, two handlers would now be
            // attached and Create would register the same server twice -> duplicate-key throw.
            Assert.DoesNotThrow(() =>
                ServerManager.Instance.Create(new ConnectionArgs("test", "irc.fake.com", false)));
        }

        [Test]
        public void TextEntry_WhileLoggerInactive_WritesNothingAndDoesNotThrow()
        {
            var server = FakeServer("a.example"); // never registered

            // The per-call guard returns before touching the (absent) log file.
            Assert.DoesNotThrow(() => TextLoggerManager.TextEntry(server, "ignored"));
            Assert.IsFalse(Directory.Exists(_tempDir), "no log files should be created while logging is off");
        }

        [Test]
        public void AddTimestamp_DelegatesToTextLogger()
        {
            TextLoggerManager.AddTimestamp = true;
            Assert.IsTrue(TextLogger.AddTimestamp);

            TextLoggerManager.AddTimestamp = false;
            Assert.IsFalse(TextLogger.AddTimestamp);
        }

        [Test]
        public void TimeFormat_DelegatesToTextLogger()
        {
            var original = TextLogger.TimeFormat;
            try
            {
                TextLoggerManager.TimeFormat = "yyyy-MM-dd";
                Assert.AreEqual("yyyy-MM-dd", TextLogger.TimeFormat);
            }
            finally
            {
                TextLogger.TimeFormat = original;
            }
        }
    }
}
