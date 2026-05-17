namespace OrtzIRC.Avalonia.Resources
{
    using System;

    internal class SocketErrorStrings
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        internal SocketErrorStrings() { }

        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("OrtzIRC.Avalonia.Resources.SocketErrorStrings", typeof(SocketErrorStrings).Assembly);
                return resourceMan;
            }
        }

        internal static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static string ConnectionRefused => ResourceManager.GetString("ConnectionRefused", resourceCulture);
        internal static string TimedOut => ResourceManager.GetString("TimedOut", resourceCulture);
    }
}
