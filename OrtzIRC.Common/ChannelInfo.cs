using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrtzIRC.Common
{
    public class ChannelInfo
    {
        /// <summary>
        /// Channel name, eg. #php
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The channel topic
        /// </summary>
        public string Topic { get; set; }
        
        public override string ToString()
        {
            //TODO: Work out how to handle prefixes. Not all channel names are prefixed with #
            return this.Name;
        }
    }
}
