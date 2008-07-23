using System;
using System.Collections.Generic;
using System.Text;

namespace FlamingIRC
{
    /// <summary>
    /// Represents a specific channel on a network
    /// </summary>
    public class Channel : Target
    {
        private List<Nick> _banList;

        public List<Nick> BanList
        {
            get { return _banList; }
            set { _banList = value; }
        }

        private ModeCollection _mode;

        public ModeCollection Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        private string _key;

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        private int _limit;

        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private List<Nick> _nickList;

        public List<Nick> NickList
        {
            get { return _nickList; }
            set { _nickList = value; }
        }
        private Topic _topic;

        public Topic Topic
        {
            get { return _topic; }
            set { _topic = value; }
        }

        public Channel()
        {
            throw new System.NotImplementedException();
        }

        public void Act()
        {
            throw new System.NotImplementedException();
        }

        public void Say()
        {
            throw new System.NotImplementedException();
        }

        public void Method()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeTopic(string topic)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }
    }
}
