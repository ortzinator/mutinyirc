namespace MutinyIRC.UI.Resources
{
    using System;

    internal class ServerStrings
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        internal ServerStrings() { }

        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                    resourceMan = new global::System.Resources.ResourceManager("MutinyIRC.UI.Resources.ServerStrings", typeof(ServerStrings).Assembly);
                return resourceMan;
            }
        }

        internal static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static string AttemptingReconnect => ResourceManager.GetString("AttemptingReconnect", resourceCulture);
        internal static string CertificateRejectedMessage => ResourceManager.GetString("CertificateRejectedMessage", resourceCulture);
        internal static string ConnectingMessage => ResourceManager.GetString("ConnectingMessage", resourceCulture);
        internal static string ConnectionFailedMessage => ResourceManager.GetString("ConnectionFailedMessage", resourceCulture);
        internal static string ConnectionFailedUnknownMessage => ResourceManager.GetString("ConnectionFailedUnknownMessage", resourceCulture);
        internal static string ConnectionLost => ResourceManager.GetString("ConnectionLost", resourceCulture);
        internal static string Disconnected => ResourceManager.GetString("Disconnected", resourceCulture);
        internal static string DisconnectSocketError => ResourceManager.GetString("DisconnectSocketError", resourceCulture);
        internal static string NickTakenMessage => ResourceManager.GetString("NickTakenMessage", resourceCulture);
        internal static string RandomNickMessage => ResourceManager.GetString("RandomNickMessage", resourceCulture);
        internal static string ServerErrorMessage => ResourceManager.GetString("ServerErrorMessage", resourceCulture);
        internal static string ServerFormTitleBar => ResourceManager.GetString("ServerFormTitleBar", resourceCulture);
        internal static string StatusConnected => ResourceManager.GetString("StatusConnected", resourceCulture);
        internal static string StatusConnectedSecure => ResourceManager.GetString("StatusConnectedSecure", resourceCulture);
        internal static string StatusConnecting => ResourceManager.GetString("StatusConnecting", resourceCulture);
        internal static string StatusDisconnected => ResourceManager.GetString("StatusDisconnected", resourceCulture);
        internal static string StatusReconnecting => ResourceManager.GetString("StatusReconnecting", resourceCulture);
        internal static string TlsHandshakeFailedMessage => ResourceManager.GetString("TlsHandshakeFailedMessage", resourceCulture);
        internal static string WarnDisconnect => ResourceManager.GetString("WarnDisconnect", resourceCulture);
    }
}
