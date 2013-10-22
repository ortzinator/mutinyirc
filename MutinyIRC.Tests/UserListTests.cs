using System;
using System.Diagnostics;
using FlamingIRC;
using NUnit.Framework;

namespace MutinyIRC.Tests
{
    /// <summary>
    ///This is a test class for UserListTest and is intended
    ///to contain all UserListTest Unit Tests
    ///</summary>
    [TestFixture]
    public class UserListTests
    {
        /// <summary>
        ///A test for Sort
        ///</summary>
        [Test]
        public void SortTest()
        {
            UserList target = new UserList();
            target.Add(new User { Nick = "Ortzinator"});
            target.Add(new User { Nick = "P90" });
            target.Add(new User { Nick = "gparent" });
            target.Add(new User { Nick = "DEADBEEF" });
            target.Add(new User { Nick = "@ChanServ" });
            target.Add(new User { Nick = "+SteveUK" });
            target.Add(new User { Nick = "@SpamServ" });
            target.Add(new User { Nick = "+HTF" });
            target.Add(new User { Nick = "+ninjers" });
            Comparison<User> comparison = (user1, user2) => user1.CompareTo(user2);
            target.Sort(comparison);
            foreach (User user in target)
            {
                Debug.WriteLine(user.Nick);
            }
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
