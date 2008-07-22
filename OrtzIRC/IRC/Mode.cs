using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrtzIRC.IRC
{
    public class ModeCollection
    {
        private List<Mode> _modes;
    }

    public enum ModeName
    {
        Operator,
        Private,
        Secret,
        InviteOnly,
        TopicOpOnly,
        NoExternalMessages,
        Moderated,
        UserLimit,
        Ban,
        Voice,
        ChannelKey,
        Invisible,
        RecieveServerNotices,
        Wallop,
        HalfOp,
    }

    public class Mode
    {
        private ModeName _mode;
    }
}
