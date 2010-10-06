namespace OrtzIRC.WPF.ViewModels
{
    using System;
    using FlamingIRC;
    using MvvmFoundation.Wpf;

    public class UserViewModel : ObservableObject, IComparable<UserViewModel>
    {
        private readonly User user;

        public string FullNick { get { return user.NamesLiteral; } }
        private Mode mode;
        public Mode Mode {get{ return mode;}}

        public UserViewModel(User user)
        {
            this.user = user;
            switch (user.Prefix)
            {
                case '@':
                    mode = Mode.Op;
                    break;
                case '+':
                    mode = Mode.Voice;
                    break;
                default:
                    mode = Mode.Regular;
                    break;
            }
        }

        public int CompareTo(UserViewModel other)
        {
            return FullNick.CompareTo(other.FullNick);
        }

        public override string ToString()
        {
            return FullNick;
        }
    }

    public enum Mode
    {
        Owner,
        Op,
        Voice,
        Regular
    }
}
