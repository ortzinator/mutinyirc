namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Views or changes channel and user modes (the /mode command), mirroring mIRC's syntax.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     In a channel window the channel is implicit, so a mode spec can be given on its own:
    ///     <c>/mode +o nick</c>, <c>/mode -o nick</c>, <c>/mode +i</c>, or bare <c>/mode</c> to
    ///     request the current channel's modes.
    ///   </para>
    ///   <para>
    ///     An explicit target also works: <c>/mode #other +o nick</c> for another channel, or
    ///     <c>/mode nick +i</c> for a user mode.
    ///   </para>
    ///   <para>
    ///     The command dispatcher coerces a leading <c>-modechars</c> token into a
    ///     <see cref="char"/>[] (stripping the sign) and a channel name into a
    ///     <see cref="ChannelInfo"/>. The overloads below cover each of those shapes and rebuild
    ///     the raw <c>MODE</c> line; a <see cref="char"/>[] parameter always means the user typed a
    ///     <c>-</c> token, so the sign is restored when rebuilding.
    ///   </para>
    /// </remarks>
    [Plugin]
    public class Mode : ICommand
    {
        // --- Current channel, no explicit target ---

        /// <summary>Requests the current channel's modes (<c>/mode</c>).</summary>
        public CommandResultInfo Execute(Channel channel)
            => Send(channel, channel.Name, string.Empty);

        /// <summary>
        ///   A bare <c>+modechars</c> spec applies to the current channel; anything else is treated
        ///   as an explicit target whose modes are requested (e.g. <c>/mode nick</c>).
        /// </summary>
        public CommandResultInfo Execute(Channel channel, string first)
            => first.StartsWith("+")
                ? Send(channel, channel.Name, first)
                : Send(channel, first, string.Empty);

        /// <summary>A leading <c>-modechars</c> with no parameters applies to the current channel.</summary>
        public CommandResultInfo Execute(Channel channel, char[] modeChars)
            => Send(channel, channel.Name, Removal(modeChars, string.Empty));

        /// <summary>
        ///   Two string tokens: <c>+modechars param</c> applies to the current channel, while
        ///   <c>target modechars</c> applies to an explicit nick (e.g. <c>/mode nick +i</c>).
        /// </summary>
        public CommandResultInfo Execute(Channel channel, string first, string rest)
            => first.StartsWith("+")
                ? Send(channel, channel.Name, $"{first} {rest}")
                : Send(channel, first, rest);

        /// <summary>An explicit nick target with a <c>-modechars</c> token (e.g. <c>/mode nick -i</c>).</summary>
        public CommandResultInfo Execute(Channel channel, string target, char[] modeChars)
            => Send(channel, target, Removal(modeChars, string.Empty));

        /// <summary>An explicit nick target with a <c>-modechars</c> token and parameters.</summary>
        public CommandResultInfo Execute(Channel channel, string target, char[] modeChars, string rest)
            => Send(channel, target, Removal(modeChars, rest));

        /// <summary>A leading <c>-modechars</c> with parameters applies to the current channel.</summary>
        public CommandResultInfo Execute(Channel channel, char[] modeChars, string rest)
            => Send(channel, channel.Name, Removal(modeChars, rest));

        // --- Explicit channel target ---

        /// <summary>Requests the modes of an explicitly named channel (<c>/mode #other</c>).</summary>
        public CommandResultInfo Execute(Channel channel, ChannelInfo target)
            => Send(channel, target.Name, string.Empty);

        /// <summary>Sets <c>+modechars</c> (and any parameters) on an explicitly named channel.</summary>
        public CommandResultInfo Execute(Channel channel, ChannelInfo target, string spec)
            => Send(channel, target.Name, spec);

        /// <summary>Removes modes (<c>-modechars</c>) on an explicitly named channel.</summary>
        public CommandResultInfo Execute(Channel channel, ChannelInfo target, char[] modeChars)
            => Send(channel, target.Name, Removal(modeChars, string.Empty));

        /// <summary>Removes modes (<c>-modechars</c>) with parameters on an explicitly named channel.</summary>
        public CommandResultInfo Execute(Channel channel, ChannelInfo target, char[] modeChars, string rest)
            => Send(channel, target.Name, Removal(modeChars, rest));

        /// <summary>Rebuilds a removal spec (<c>-modechars [params]</c>) from a coerced char array.</summary>
        private static string Removal(char[] modeChars, string rest)
            => string.IsNullOrWhiteSpace(rest)
                ? "-" + new string(modeChars)
                : "-" + new string(modeChars) + " " + rest;

        private static CommandResultInfo Send(Channel channel, string target, string modeArgs)
        {
            channel.Server.SendMode(target, modeArgs);
            return CommandResultInfo.Success(string.Empty);
        }
    }
}
