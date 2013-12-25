using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FakeItEasy;
using FlamingIRC;

namespace FlamingIRC.Tests
{
    [TestFixture]
    public class UserModeValidatorTests
    {
        [Test]
        public void IsValid_ValidMode_ReturnsTrue()
        {
            Assert.IsTrue(UserModeValidator.IsValid('@'));
            Assert.IsTrue(UserModeValidator.IsValid('+'));
            Assert.IsTrue(UserModeValidator.IsValid('%'));
            Assert.IsTrue(UserModeValidator.IsValid('&'));
            Assert.IsTrue(UserModeValidator.IsValid('~'));
        }

        [Test]
        public void IsValid_InvalidMode_ReturnsFalse()
        {
            Assert.IsFalse(UserModeValidator.IsValid('X'));
            Assert.IsFalse(UserModeValidator.IsValid('\0'));
        }
    }
}