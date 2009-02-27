namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class ChannelInfo
    {
        public ChannelInfo(string name)
        {
            if (Rfc2812Util.IsValidChannelName(name))
            {
                Name = name;
            }
            else
            {
                throw new Exception("Not a valid channel name."); //TODO: Make exception less lame
                // TODO: P90: Do we really need to throw and generate a full stack trace and snapshot
                //            just because of a potential mistake? Maybe a check for bool would be faster
                //            but I'm not sure how good C# handles exceptions.
            }
        }

        /// <summary>
        /// Channel name, eg. #php
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The channel topic
        /// </summary>
        public string Topic { get; set; }

        public override string ToString()
        {
            //TODO: Work out how to handle prefixes. Not all channel names are prefixed with #
            return Name;
        }
    }
}
