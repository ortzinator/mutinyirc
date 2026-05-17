namespace MutinyIRC.UI.Resources
{
    using System;

    internal class ChannelStrings
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        internal ChannelStrings() { }

        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("MutinyIRC.UI.Resources.ChannelStrings", typeof(ChannelStrings).Assembly);
                return resourceMan;
            }
        }

        internal static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static string Action => ResourceManager.GetString("Action", resourceCulture);
        internal static string ErrorMessageCaption => ResourceManager.GetString("ErrorMessageCaption", resourceCulture);
        internal static string Joined => ResourceManager.GetString("Joined", resourceCulture);
        internal static string Kick => ResourceManager.GetString("Kick", resourceCulture);
        internal static string NickChange => ResourceManager.GetString("NickChange", resourceCulture);
        internal static string Part => ResourceManager.GetString("Part", resourceCulture);
        internal static string PartWithReason => ResourceManager.GetString("PartWithReason", resourceCulture);
        internal static string PublicMessage => ResourceManager.GetString("PublicMessage", resourceCulture);
        internal static string Quit => ResourceManager.GetString("Quit", resourceCulture);
        internal static string TopicRecieved => ResourceManager.GetString("TopicRecieved", resourceCulture);
    }
}
