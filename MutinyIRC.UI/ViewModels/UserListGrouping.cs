using System;
using System.Collections.Generic;
using System.Linq;

namespace MutinyIRC.UI.ViewModels;

/// <summary>
/// Turns a channel's users into the flat, grouped row list the user list renders: a
/// <see cref="UserGroupHeaderViewModel" /> followed by its <see cref="UserViewModel" />s, for each
/// group that has anyone in it. Kept separate from <see cref="ChannelViewModel" /> so the grouping,
/// ordering and filter rules can be tested without a live channel.
/// </summary>
public static class UserListGrouping
{
    /// <summary>
    /// Groups in display order. Owners and admins sit with the ops — the '~' and '&amp;' glyphs
    /// already show the rank difference, so splitting them out would leave one-line sections.
    /// </summary>
    private static readonly (string Name, Mode[] Modes)[] Groups =
    {
        ("Operators", new[] { Mode.Owner, Mode.Op }),
        ("Half-ops",  new[] { Mode.HalfOp }),
        ("Voiced",    new[] { Mode.Voice }),
        ("Members",   new[] { Mode.Regular }),
    };

    /// <summary>
    /// Builds the row list. <paramref name="filter" /> is an optional case-insensitive substring
    /// match on the bare nick; a group with no surviving members drops out entirely, header and all.
    /// </summary>
    public static IReadOnlyList<IUserListRow> Build(IEnumerable<UserViewModel> users, string? filter = null)
    {
        var rows = new List<IUserListRow>();

        foreach ((string name, Mode[] modes) in Groups)
        {
            List<UserViewModel> members = users
                .Where(u => modes.Contains(u.Mode) && Matches(u, filter))
                .OrderBy(u => u.Nick, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (members.Count == 0)
                continue;

            rows.Add(new UserGroupHeaderViewModel(name, members.Count));
            rows.AddRange(members);
        }

        return rows;
    }

    private static bool Matches(UserViewModel user, string? filter) =>
        string.IsNullOrWhiteSpace(filter)
        || user.Nick.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);
}
