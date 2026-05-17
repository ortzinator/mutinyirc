namespace MutinyIRC.UI.Resources
{
    using System;

    internal class CommonStrings
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        internal CommonStrings() { }

        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("MutinyIRC.UI.Resources.CommonStrings", typeof(CommonStrings).Assembly);
                return resourceMan;
            }
        }

        internal static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static string CommandErrorMessage => ResourceManager.GetString("CommandErrorMessage", resourceCulture);
        internal static string DialogCaption => ResourceManager.GetString("DialogCaption", resourceCulture);
        internal static string MainExitPrompt => ResourceManager.GetString("MainExitPrompt", resourceCulture);
        internal static string MainExitPromptCaption => ResourceManager.GetString("MainExitPromptCaption", resourceCulture);
        internal static string PrivateNotice => ResourceManager.GetString("PrivateNotice", resourceCulture);
    }
}
