namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class ChannelInfo
    {
        /// <summary>
        /// Channel name, eg. #php
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The channel topic
        /// </summary>
        public string Topic { get; set; }

        public ChannelInfo(string name)
        {
            if (Rfc2812Util.IsValidChannelName(name))
            {
                Name = name;
            }
            else
            {
                throw new Exception("Not a valid channel name."); //TODO: Make exception less lame
            }
        }

        public override string ToString()
        {
            //TODO: Work out how to handle prefixes. Not all channel names are prefixed with #
            return this.Name;
        }
    }
}
