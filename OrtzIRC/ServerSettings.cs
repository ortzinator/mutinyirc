using System;
using System.Collections.Generic;
using System.Text;

namespace OrtzIRC
{
    public class IRCSettingsManager
    {
        private static IRCSettingsManager _instance;
        public static IRCSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IRCSettingsManager();
                }
                return _instance;
            }
        }

        protected IRCSettingsManager() { }


    }

    public struct NetworkSettings 
    {

        private string _name;
    }

    public struct ServerSettings
    {
        private string _URI;
        private string _description;
        private int _port;
    }
}
