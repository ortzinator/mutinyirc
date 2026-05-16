namespace FlamingIRC
{
    public interface ISender
    {
        void Join(string channel);
        void Join(string channel, string password);
        void Nick(string newNick);
        void Names(params string[] channels);
        void AllNames();
        void List(params string[] channels);
        void AllList();
        void ChangeTopic(string channel, string newTopic);
        void ClearTopic(string channel);
        void RequestTopic(string channel);
        void Part(string reason, params string[] channels);
        void Part(string channel);
        void Ping();
        void PublicNotice(string channel, string message);
        void PrivateNotice(string nick, string message);
        void PublicMessage(string channel, string message);
        void PrivateMessage(string nick, string message);
        void Invite(string who, string channel);
        void Kick(string channel, string reason, params string[] nicks);
        void Ison(string nick);
        void Who(string mask, bool operatorsOnly);
        void AllWho();
        void Whois(string nick);
        void Away(string message);
        void UnAway();
        void Whowas(string nick);
        void Whowas(string nick, int count);
        void RequestUserModes();
        void ChangeUserMode(ModeAction action, UserMode mode);
        void ChangeChannelMode(string channel, ModeAction action, ChannelMode mode, string param);
        void RequestChannelList(string channel, ChannelMode mode);
        void RequestChannelModes(string channel);
        void Action(string channel, string description);
        void PrivateAction(string nick, string description);
        void Register(string newNick);
        void Raw(string message);
        void Version();
        void Version(string targetServer);
        void Motd();
        void Motd(string targetServer);
        void Time();
        void Time(string targetServer);
        void Wallops(string message);
        void Info();
        void Info(string target);
        void Admin();
        void Admin(string target);
        void Lusers();
        void Lusers(string hostMask, string targetServer);
        void Links();
        void Links(params string[] masks);
        void Stats(StatsQuery query);
        void Stats(StatsQuery query, string targetServer);
        void Kill(string nick, string reason);
    }
}
