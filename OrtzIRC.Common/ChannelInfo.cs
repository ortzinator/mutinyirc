namespace OrtzIRC.Common
{
    using System;
    using FlamingIRC;

    public class ChannelInfo
    {
        public ChannelInfo(string name)
        {
            if (!Rfc2812Util.IsValidChannelName(name))
                throw new ArgumentException("Invaid channel name", "name");

            Name = name;
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
            return Name;
        }
    }
}
