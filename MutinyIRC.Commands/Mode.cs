namespace MutinyIRC.Commands
{
    using MutinyIRC.Common;
    using PluginFramework;

    /// <summary>
    ///   Views or changes channel and user modes (the /mode command), mirroring mIRC's syntax.
    /// </summary>
    /// <remarks>
    ///   In a channel window the channel is implicit: a mode spec stands alone (<c>/mode +o nick</c>,
    ///   <c>/mode -i</c>) and bare <c>/mode</c> requests the current channel's modes. An explicit
    ///   target also works (<c>/mode #other +o nick</c>, <c>/mode nick +i</c>). The tail is passed
    ///   through verbatim, so a leading <c>-</c> stays part of the mode string.
    /// </remarks>
    [Plugin("Mode", "Views or changes channel and user modes. Usage: /mode [target] [modes]")]
    public class Mode : ICommand
    {
        /// <summary>Requests the current channel's modes (bare <c>/mode</c>).</summary>
        public CommandResultInfo Execute(Channel channel) => Execute(channel, string.Empty);

        /// <summary>
        ///   A leading <c>+</c>/<c>-</c> token is a mode spec for the current channel; anything else
        ///   is an explicit target (channel or nick) followed by its own optional spec.
        /// </summary>
        [RawArguments]
        public CommandResultInfo Execute(Channel channel, string rest)
        {
            string target, modeArgs;
            if (rest.Length == 0 || rest[0] == '+' || rest[0] == '-')
            {
                target = channel.Name;          // implicit current channel
                modeArgs = rest;
            }
            else
            {
                int sep = rest.IndexOf(' ');     // explicit target, spec is the remainder
                target = sep < 0 ? rest : rest.Substring(0, sep);
                modeArgs = sep < 0 ? string.Empty : rest.Substring(sep + 1);
            }

            channel.Server.SendMode(target, modeArgs);
            return CommandResultInfo.Success(string.Empty);
        }
    }
}
