using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.Commands;

/// <summary>
/// Requests or sets the topic of the current channel (the /topic command).
/// </summary>
[Plugin("Topic", "Views or sets the current channel's topic. Usage: /topic [new topic]")]
public class Topic : ICommand
{
    /// <summary>
    /// Requests the current topic for the channel.
    /// </summary>
    public void Execute(Channel channel)
    {
        channel.RequestTopic();
    }

    /// <summary>
    /// Sets the channel topic.
    /// </summary>
    public void Execute(Channel channel, string topic)
    {
        channel.SetTopic(topic);
    }
}
