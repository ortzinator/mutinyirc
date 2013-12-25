using System;
using FakeItEasy;
using NUnit.Framework;
using OrtzIRC.WPF.ViewModels;
using FlamingIRC;

namespace MutinyIRC.WPF.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private UserViewModel _uvm;

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
            _uvm = null;
        }

        [Test]
        public void Construct_UserWithoutPrefix_ModeIsRegular()
        {
            var user = A.Fake<User>();
            user.Prefix = '\0';

            _uvm =  new UserViewModel(user);

            Assert.AreEqual(_uvm.Mode, Mode.Regular);
        }
    }
}
