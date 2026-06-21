using System;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests
{
    /// <summary>
    ///   Pins <see cref="Sender"/>'s command-buffer hygiene: a call that fails validation must not
    ///   leave half-built text behind in the shared <c>Buffer</c>, or the <em>next</em> command
    ///   sent on the same connection comes out corrupted. Most senders append their verb before
    ///   running validation, so the contract is "every throw path clears the buffer first."
    ///   <para>
    ///   Observation is purely behavioural: a real but never-connected <see cref="Connection"/>
    ///   swallows the socket failure inside <c>SendCommand</c> yet still fires
    ///   <see cref="Connection.RawMessageSent"/> with whatever the buffer held and then clears it.
    ///   So the wire text of a follow-up command reveals any residue from the failed call without
    ///   needing access to the internal buffer.
    ///   </para>
    /// </summary>
    [TestFixture]
    public class SenderBufferTests
    {
        private Connection _connection;
        private ISender _sender;
        private string _lastSent;

        [SetUp]
        public void SetUp()
        {
            // enableCtcp/enableDcc off, never Connect()ed: SendCommand's socket write fails and is
            // swallowed, but the buffer is still built, surfaced via RawMessageSent, and cleared.
            _connection = new Connection(new ConnectionArgs("test", "irc.fake.com", false), false, false);
            _sender = _connection.Sender;
            _connection.RawMessageSent += (_, e) => _lastSent = e.Data;
        }

        // Sends a known-good command after a failed one; its wire text must be exactly this and
        // nothing more, proving the failed call left no residue in the buffer.
        private void AssertNextCommandIsClean()
        {
            _lastSent = null;
            _sender.Whois("bob");
            Assert.AreEqual("WHOIS bob", _lastSent,
                "The previous (failed) command left text in the shared buffer, corrupting this one.");
        }

        [Test]
        public void Nick_WithInvalidNick_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Nick("not a nick"));
            AssertNextCommandIsClean();
        }

        [Test]
        public void Join_WithNullPassword_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Join("#chan", null));
            AssertNextCommandIsClean();
        }

        [Test]
        public void Away_WithEmptyMessage_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Away(""));
            AssertNextCommandIsClean();
        }

        [Test]
        public void Wallops_WithEmptyMessage_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Wallops(""));
            AssertNextCommandIsClean();
        }

        [Test]
        public void PublicMessage_WithEmptyMessage_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.PublicMessage("#chan", ""));
            AssertNextCommandIsClean();
        }

        [Test]
        public void Kick_WithEmptyReason_ThrowsAndLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Kick("#chan", "", "victim"));
            AssertNextCommandIsClean();
        }

        // The important append-then-validate case: "LUSERS <hostmask>" is appended in full and only
        // then found too long, so the throw path must clear what it already wrote.
        [Test]
        public void Lusers_WhenArgumentsTooLong_AppendsVerbThenThrowsButLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Lusers(new string('a', 600), null));
            AssertNextCommandIsClean();
        }

        // Same shape via a different command: "STATS l <target>" is built before the length check.
        [Test]
        public void Stats_WhenTargetTooLong_AppendsVerbThenThrowsButLeavesBufferClean()
        {
            Assert.Throws<ArgumentException>(() => _sender.Stats(StatsQuery.Connections, new string('a', 600)));
            AssertNextCommandIsClean();
        }

        // Regression guard: Links must bounds-check masks before indexing masks[0]. An empty
        // (non-null) array used to append "LINKS " and then throw IndexOutOfRangeException without
        // clearing, poisoning the next command. It now sends a bare LINKS (like the no-arg overload)
        // and leaves the buffer clean.
        [Test]
        public void Links_WithEmptyArray_SendsBareLinksAndLeavesBufferClean()
        {
            _lastSent = null;
            Assert.DoesNotThrow(() => _sender.Links(new string[0]));
            Assert.AreEqual("LINKS", _lastSent);

            AssertNextCommandIsClean();
        }
    }
}
