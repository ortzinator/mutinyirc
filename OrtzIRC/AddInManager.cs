using System;
using System.Collections.Generic;
using System.Text;
using System.AddIn;
using System.AddIn.Hosting;

namespace OrtzIRC
{
    internal class AddInManager
    {
        private static AddInManager _instance;

        public static AddInManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AddInManager();
                    //_tokens = AddInStore.FindAddIns(typeof(HostView.NumberProcessorHostView), path);
                }
                return _instance;
            }
        }

        internal static IList<AddInToken> AddInTokens { get; private set; } //Is internal necessary?
    }
}
