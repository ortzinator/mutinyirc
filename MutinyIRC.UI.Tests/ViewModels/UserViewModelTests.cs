using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class UserViewModelTests
{
    [TestCase("~founder", Mode.Owner)]
    [TestCase("&admin", Mode.Owner)]
    [TestCase("@Alice", Mode.Op)]
    [TestCase("%Helper", Mode.HalfOp)]
    [TestCase("+Bob", Mode.Voice)]
    [TestCase("Charlie", Mode.Regular)]
    public void UserViewModel_MapsStatusPrefixToMode(string namesLiteral, Mode expected)
    {
        var vm = new UserViewModel(User.FromNames(namesLiteral));

        Assert.That(vm.Mode, Is.EqualTo(expected));
    }

    [TestCase("~founder", "~")]
    [TestCase("&admin", "&")]
    [TestCase("@Alice", "@")]
    [TestCase("%Helper", "%")]
    [TestCase("+Bob", "+")]
    public void PrefixGlyph_IsTheStatusSymbol(string namesLiteral, string expected)
    {
        var vm = new UserViewModel(User.FromNames(namesLiteral));

        Assert.That(vm.PrefixGlyph, Is.EqualTo(expected));
    }

    /// <summary>
    /// Statusless users still occupy the prefix column so every nick in the list starts on the
    /// same x. A blank there would leave the column ragged.
    /// </summary>
    [Test]
    public void PrefixGlyph_IsAMiddleDot_ForUsersWithNoStatus()
    {
        var vm = new UserViewModel(User.FromNames("Charlie"));

        Assert.That(vm.PrefixGlyph, Is.EqualTo("·"));
    }

    [Test]
    public void Tag_IsAbsentByDefault_SoMostRowsShowNoBadge()
    {
        var vm = new UserViewModel(User.FromNames("Charlie"));

        Assert.That(vm.Tag, Is.Null);
        Assert.That(vm.HasTag, Is.False);
    }

    [Test]
    public void Tag_ShowsWhenGiven()
    {
        var vm = new UserViewModel(User.FromNames("@ChanServ"), "bot");

        Assert.That(vm.Tag, Is.EqualTo("bot"));
        Assert.That(vm.HasTag, Is.True);
    }

    [Test]
    public void Nick_StripsThePrefix_SoCommandsTargetTheBareName()
    {
        var vm = new UserViewModel(User.FromNames("@Alice"));

        Assert.That(vm.Nick, Is.EqualTo("Alice"));
        Assert.That(vm.FullNick, Is.EqualTo("@Alice"));
    }
}
