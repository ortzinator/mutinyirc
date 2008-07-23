using System;
using System.Collections.Generic;
using System.Text;

namespace FlamingIRC
{
    public delegate void PingEventHandler(Message message);

    public delegate void JoinEventHandler(Nick joiner, Channel channel);

    public delegate void PartEventHandler(Nick parter, Channel channel, string reason);

    public delegate void NickEventHandler(Nick nick, string newNick);

    public delegate void PrivateMessageEventHandler(Nick nick, string message);
    
    public delegate void PublicMessageEventHandler(Nick nick, Channel channel, string message);

    public delegate void TopicEventHandler(Nick nick, Channel channel, string newTopic);

    public delegate void QuitEventHandler(Nick nick, string reason);

    public delegate void KickEventHandler(Nick nick, Channel channel, Nick kickee, string reason);

    public delegate void PrivateNoticeEventHandler(Nick nick, string message);

    public delegate void PublicNoticeEventHandler(Nick nick, Channel channel, string message);

    public delegate void ChannelModeChangeEventHandler(Nick nick, Channel channel, ChannelModeInfo[] modes);
}
