using System;
using System.Reflection;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using MutinyIRC.Common;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///   Guards that <see cref="Server.UnhookEvents"/> stays a perfect mirror of
    ///   <see cref="Server.HookEvents"/>. These are two hand-maintained parallel lists that have
    ///   drifted before (an unhook line was forgotten, leaking subscriptions on every teardown),
    ///   so the count-based test below fails the moment they diverge again — for any event, with
    ///   no per-event wiring to keep in sync.
    /// </summary>
    [TestFixture]
    public class ServerEventHookingTests
    {
        private static Connection FakeConnection()
        {
            var args = new ConnectionArgs("test", "irc.fake.com", false);
            return A.Fake<Connection>(x => x.WithArgumentsForConstructor(new object[] { args, false, false }));
        }

        [Test]
        public void UnhookEvents_RemovesEveryHandlerHookEventsAdded()
        {
            var conn = FakeConnection();

            // Snapshot before the Server wires anything up. Connection may subscribe to its own
            // Listener internally; that baseline stays constant, so the delta isolates the Server.
            int baseline = TotalSubscriptions(conn);

            var server = new Server(conn); // ctor calls HookEvents
            int hooked = TotalSubscriptions(conn);
            Assert.Greater(hooked, baseline, "HookEvents should have added subscriptions");

            server.UnhookEvents();
            int afterUnhook = TotalSubscriptions(conn);

            Assert.AreEqual(baseline, afterUnhook,
                "UnhookEvents must remove exactly the handlers HookEvents added. A mismatch means " +
                "the two lists have drifted — every event HookEvents touches needs a matching -= in UnhookEvents.");
        }

        [Test]
        public void NickError_AfterUnhookEvents_DoesNotFire()
        {
            // OnNickError was one of the handlers missing from UnhookEvents before the fix, so it
            // kept firing after teardown. This pins that specific regression behaviorally.
            var server = new Server(FakeConnection());
            server.UnhookEvents();

            bool fired = false;
            server.NickError += (_, _) => fired = true;

            server.Connection.Listener.Parse(":server.name 433 test newnick :Nickname is already in use");

            Assert.IsFalse(fired, "A NickError after UnhookEvents means the handler was never detached");
        }

        // Sums the invocation-list lengths of every field-like event on the Connection and its
        // Listener. Field-like events compile to a private backing delegate field of the same name,
        // which we read reflectively (walking the type hierarchy so FakeItEasy proxies work too).
        private static int TotalSubscriptions(Connection conn)
        {
            return CountEventSubscriptions(conn) + CountEventSubscriptions(conn.Listener);
        }

        private static int CountEventSubscriptions(object target)
        {
            int total = 0;
            Type type = target.GetType();
            foreach (EventInfo evt in type.GetEvents(BindingFlags.Public | BindingFlags.Instance))
            {
                FieldInfo backing = FindBackingField(type, evt.Name);
                if (backing?.GetValue(target) is Delegate del)
                    total += del.GetInvocationList().Length;
            }
            return total;
        }

        private static FieldInfo FindBackingField(Type type, string name)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                FieldInfo field = t.GetField(name,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
            }
            return null;
        }
    }
}
