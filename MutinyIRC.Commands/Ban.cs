using System;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Bans a user or mask from a channel (the /ban command).
/// </summary>
/// <remarks>
/// Usage: <c>/ban [#channel] [-k] [-r] &lt;nick|mask&gt; [reason]</c>. In a channel window the
/// channel is implicit; from a private message or server window an explicit
/// <c>#channel</c> you're on is required. A target containing <c>!</c>, <c>@</c>, or
/// <c>*</c> is treated as a literal mask; otherwise it's a nick resolved against the
/// channel's user list to <c>*!*@host</c> (falling back to <c>nick!*@*</c> when the host
/// isn't known). <c>-k</c> also kicks the nick after banning; <c>-r</c> removes the ban
/// (<c>-b</c>) instead of setting it.
/// </remarks>
[Plugin("Ban", "Bans a user/mask from a channel. Usage: /ban [#channel] [-k] [-r] <nick|mask> [reason]")]
public class Ban : ICommand
{
    private const string Usage = "Usage: /ban [#channel] [-k] [-r] <nick|mask> [reason]";

    /// <summary>Bans from the channel this command was run in.</summary>
    [RawArguments]
    public CommandResultInfo Execute(Channel channel, string rest)
        => Run(channel.Server, channel, rest);

    /// <summary>Bans from an explicit channel named in the arguments (PM/server window).</summary>
    [RawArguments]
    public CommandResultInfo Execute(MessageContext context, string rest)
        => Run(context.OwningServer, null, rest);

    private static CommandResultInfo Run(Server server, Channel defaultChannel, string rest)
    {
        string[] tokens = (rest ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int i = 0;

        // Explicit channel token, otherwise the window's channel.
        string channelName;
        if (i < tokens.Length && (tokens[i][0] == '#' || tokens[i][0] == '&'))
            channelName = tokens[i++];
        else
            channelName = defaultChannel?.Name;

        if (channelName == null)
            return CommandResultInfo.Fail(Usage);

        if (!server.Channels.TryGetValue(channelName, out Channel channel))
            return CommandResultInfo.Fail($"You must be on {channelName} to set bans there.");

        // Switches: -k (kick-ban), -r (remove), combinable as -kr.
        bool kick = false, remove = false;
        while (i < tokens.Length && tokens[i].Length > 1 && tokens[i][0] == '-')
        {
            foreach (char c in tokens[i].Substring(1))
            {
                if (c == 'k') kick = true;
                else if (c == 'r') remove = true;
                else return CommandResultInfo.Fail(Usage);
            }
            i++;
        }

        if (i >= tokens.Length)
            return CommandResultInfo.Fail(Usage);

        string target = tokens[i++];
        string reason = i < tokens.Length
            ? string.Join(' ', tokens, i, tokens.Length - i)
            : server.UserNick;

        bool isMask = target.IndexOfAny(new[] { '!', '@', '*' }) >= 0;
        string mask = isMask ? target : channel.ResolveBanMask(target);

        if (remove)
        {
            if (kick)
                return CommandResultInfo.Fail("Can't kick (-k) and remove a ban (-r) at once.");
            channel.Unban(mask);
        }
        else
        {
            channel.Ban(mask);
            if (kick && !isMask)
                channel.Kick(target, reason);
        }

        return CommandResultInfo.Success(string.Empty);
    }
}
