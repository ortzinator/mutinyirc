using System.Collections.Generic;
using System.Linq;
using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Covers the row list the user list renders: which groups appear, in what order, how users sort
/// inside them, and what the filter keeps. The view binds straight to this, so a break here is a
/// break on screen.
/// </summary>
[TestFixture]
public class UserListGroupingTests
{
    private static List<UserViewModel> Users(params string[] namesLiterals) =>
        namesLiterals.Select(n => new UserViewModel(User.FromNames(n))).ToList();

    private static List<string> HeaderNames(IReadOnlyList<IUserListRow> rows) =>
        rows.OfType<UserGroupHeaderViewModel>().Select(h => h.Name).ToList();

    private static List<string> Nicks(IReadOnlyList<IUserListRow> rows) =>
        rows.OfType<UserViewModel>().Select(u => u.Nick).ToList();

    [Test]
    public void Build_OrdersGroupsByRank()
    {
        var rows = UserListGrouping.Build(Users("plain", "+voiced", "%halfop", "@op"));

        Assert.That(HeaderNames(rows), Is.EqualTo(new[] { "OPERATORS", "HALF-OPS", "VOICED", "MEMBERS" }));
    }

    [Test]
    public void Build_OmitsGroupsWithNoMembers()
    {
        var rows = UserListGrouping.Build(Users("@op", "plain"));

        Assert.That(HeaderNames(rows), Is.EqualTo(new[] { "OPERATORS", "MEMBERS" }),
            "A group with nobody in it must not leave a bare heading behind");
    }

    [Test]
    public void Build_PutsOwnersAndAdminsWithTheOperators()
    {
        var rows = UserListGrouping.Build(Users("~founder", "&admin", "@op"));

        Assert.That(HeaderNames(rows), Is.EqualTo(new[] { "OPERATORS" }));
        Assert.That(Nicks(rows), Is.EqualTo(new[] { "admin", "founder", "op" }));
    }

    [Test]
    public void Build_CountsOnlyItsOwnGroup()
    {
        var rows = UserListGrouping.Build(Users("@op", "plain", "other"));

        var headers = rows.OfType<UserGroupHeaderViewModel>().ToList();
        Assert.That(headers[0].Count, Is.EqualTo(1));
        Assert.That(headers[1].Count, Is.EqualTo(2));
    }

    [Test]
    public void Build_SortsInsideEachGroupIgnoringCase()
    {
        var rows = UserListGrouping.Build(Users("zed", "Alice", "bob"));

        Assert.That(Nicks(rows), Is.EqualTo(new[] { "Alice", "bob", "zed" }));
    }

    [Test]
    public void Build_HeaderPrecedesItsOwnMembers()
    {
        var rows = UserListGrouping.Build(Users("@op", "plain"));

        Assert.That(rows[0], Is.InstanceOf<UserGroupHeaderViewModel>());
        Assert.That(((UserViewModel)rows[1]).Nick, Is.EqualTo("op"));
        Assert.That(rows[2], Is.InstanceOf<UserGroupHeaderViewModel>());
        Assert.That(((UserViewModel)rows[3]).Nick, Is.EqualTo("plain"));
    }

    [Test]
    public void Build_FilterMatchesAnywhereInTheNick()
    {
        var rows = UserListGrouping.Build(Users("alice", "malice", "bob"), "lic");

        Assert.That(Nicks(rows), Is.EqualTo(new[] { "alice", "malice" }));
    }

    [Test]
    public void Build_FilterIgnoresCase()
    {
        var rows = UserListGrouping.Build(Users("Alice", "bob"), "ALI");

        Assert.That(Nicks(rows), Is.EqualTo(new[] { "Alice" }));
    }

    [Test]
    public void Build_FilterDropsEmptiedGroupsAndRecountsTheRest()
    {
        var rows = UserListGrouping.Build(Users("@op", "alice", "malice"), "lic");

        Assert.That(HeaderNames(rows), Is.EqualTo(new[] { "MEMBERS" }),
            "Filtering everyone out of a group must remove its heading too");
        Assert.That(rows.OfType<UserGroupHeaderViewModel>().Single().Count, Is.EqualTo(2),
            "The heading count must report the filtered total, not the channel total");
    }

    [Test]
    public void Build_BlankFilterKeepsEveryone()
    {
        Assert.That(Nicks(UserListGrouping.Build(Users("alice", "bob"), "   ")),
            Is.EqualTo(new[] { "alice", "bob" }));
        Assert.That(Nicks(UserListGrouping.Build(Users("alice", "bob"), null)),
            Is.EqualTo(new[] { "alice", "bob" }));
    }

    [Test]
    public void Build_FilterIgnoresSurroundingWhitespace()
    {
        Assert.That(Nicks(UserListGrouping.Build(Users("alice", "bob"), "  ali  ")),
            Is.EqualTo(new[] { "alice" }));
    }

    [Test]
    public void Build_NoMatches_ReturnsNothingAtAll()
    {
        var rows = UserListGrouping.Build(Users("@op", "alice"), "zzz");

        Assert.That(rows, Is.Empty);
    }

    [Test]
    public void HeadersAreNotSelectable_ButUsersAre()
    {
        var rows = UserListGrouping.Build(Users("@op"));

        Assert.That(rows[0].IsSelectable, Is.False,
            "ChannelView binds ListBoxItem.IsHitTestVisible to IsSelectable to keep headers inert");
        Assert.That(rows[1].IsSelectable, Is.True);
    }
}
