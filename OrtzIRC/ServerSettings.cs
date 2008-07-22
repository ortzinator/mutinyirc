using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    public class ServerSettings
    {
        private ServerSettings _instance;

        public static ServerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerSettings();
                }
                return _instance;
            }
            set { _instance = value; }
        }

        protected ServerSettings()
        {
            throw new System.NotImplementedException();
        }
    }
}
