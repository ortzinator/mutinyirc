using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Parts a channel
/// </summary>
[Plugin("Part", "Leaves a channel. Usage: /part [#channel] [message]")]
public class Part : ICommand
{
    private const string fallbackMessage = "Goodbye!";

    private static string DefaultMessage() => RandomMessages.Instance.GetMessage("part") ?? fallbackMessage;

    /// <summary>
    /// Parts the current channel with a message
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message"></param>
    public void Execute(Channel context, string message)
    {
        context.Part(message);
    }

    /// <summary>
    /// Parts the current channel
    /// </summary>
    /// <param name="context"></param>
    public void Execute(Channel context)
    {
        Execute(context, DefaultMessage());
    }

    /// <summary>
    /// Parts the specified channel
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    public void Execute(Channel context, ChannelInfo channel)
    {
        Execute(context.Server, channel, DefaultMessage());
    }

    /// <summary>
    /// Parts the specified channel with the specified message
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public void Execute(Channel context, ChannelInfo channel, string message)
    {
        Execute(context.Server, channel, message);
    }

    /// <summary>
    /// Parts the specified channel
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    public void Execute(Server context, ChannelInfo channel)
    {
        Execute(context, channel, DefaultMessage());
    }

    /// <summary>
    /// Parts the specified channel with the specified message
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public void Execute(Server context, ChannelInfo channel, string message)
    {
        if (context.InChannel(channel.Name))
            context.Channels[channel.Name].Part(message);
    }

    /// <summary>
    /// Parts the specified channel
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    public void Execute(PrivateMessageSession context, ChannelInfo channel)
    {
        Execute(context.Server, channel, DefaultMessage());
    }

    /// <summary>
    /// Parts the specified channel with the specified message
    /// </summary>
    /// <param name="context"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public void Execute(PrivateMessageSession context, ChannelInfo channel, string message)
    {
        Execute(context.Server, channel, message);
    }
}
