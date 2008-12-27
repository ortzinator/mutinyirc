using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrtzIRC.Common
{
    public class ChannelInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        public override string ToString()
        {
            return string.Format("#{0}", this.Name);
        }
    }
}
