using System;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;

namespace FlamingIRC.Tests
{
    [TestFixture]
    public class UserTests
    {
        private User _user;

        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void Teardown()
        {
            _user = null;
        }

        [Test]
        public void FromNames_ValidNickWithModeChar_UserReturned()
        {
            User u = User.FromNames("@Ortzinator");
            Assert.AreEqual(u.Nick, "Ortzinator");
            Assert.AreEqual(u.Prefix, '@');
        }

        [Test]
        public void FromNames_ValidNick_UserReturned()
        {
            User u = User.FromNames("Ortzinator");
            Assert.AreEqual(u.Nick, "Ortzinator");
            Assert.AreEqual(u.Prefix, '\0');
        }

        [Test]
        public void PrefixSetter_NullChar_NoException()
        {
            User user = new User();
            user.Prefix = '\0';
        }

        [Test]
        public void PrefixSetter_InvalidChar_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate () {
                User user = new User();
                user.Prefix = 'X';
            });
        }
    }
}
