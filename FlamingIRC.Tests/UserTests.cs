using System;
using FakeItEasy;
using FlamingIRC;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace FlamingIRC.Tests;

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
        Assert.Throws<ArgumentOutOfRangeException>(delegate ()
        {
            User user = new User();
            user.Prefix = 'X';
        });
    }

    [Test]
    public void Prefix_OpAndVoice_ReturnsHighestRankedSymbol()
    {
        User user = new User();
        user.AddStatus('+');
        user.AddStatus('@');

        Assert.AreEqual('@', user.Prefix);
        Assert.IsTrue(user.HasStatus('+'));
        Assert.IsTrue(user.HasStatus('@'));
    }

    [Test]
    public void RemoveStatus_HighestSymbol_PrefixFallsBackToNextHighest()
    {
        User user = new User();
        user.AddStatus('@');
        user.AddStatus('+');

        user.RemoveStatus('@');

        Assert.AreEqual('+', user.Prefix);
        Assert.IsFalse(user.HasStatus('@'));
    }

    [Test]
    public void PrefixSetter_ValidSymbol_ReplacesAllStatuses()
    {
        User user = new User();
        user.AddStatus('@');
        user.AddStatus('+');

        user.Prefix = '%';

        Assert.AreEqual('%', user.Prefix);
        Assert.IsFalse(user.HasStatus('@'), "Setting Prefix must replace, not merge, statuses");
        Assert.IsFalse(user.HasStatus('+'));
    }

    [Test]
    public void AddStatus_InvalidSymbol_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(delegate ()
        {
            new User().AddStatus('X');
        });
    }

    [Test]
    public void FromNames_MultiplePrefixSymbols_AllStatusesTracked()
    {
        User u = User.FromNames("@+Ortzinator");

        Assert.AreEqual("Ortzinator", u.Nick);
        Assert.AreEqual('@', u.Prefix);
        Assert.IsTrue(u.HasStatus('@'));
        Assert.IsTrue(u.HasStatus('+'));
    }
}
