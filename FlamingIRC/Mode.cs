using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace FlamingIRC
{
    public class ModeCollection
    {
        private List<Mode> _modes;
    }

    public enum ModeName
    {
        ChannelOperator,
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
        private ModeAction _modeAction;
    }

    public enum ModeAction
    {
        Add,
        Remove,
    }

    public class ChannelModeInfo
    {
        private ModeName _modeName;
        private ModeAction _modeAction;
    }

    public class ChannelModeCollection : IEnumerable<ChannelModeInfo>
    {
        private List<ChannelModeInfo> _modes;

        public IEnumerator<ChannelModeInfo> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public enum UserMode
    {
        Away,
        Wallops,
        Invisible,
        Operator,
        Restricted,
    }
}
