namespace OrtzIRC.WPF
{
    using System.Windows;

    public class CommandEventArgs : RoutedEventArgs
    {
        public CommandEventArgs(RoutedEvent ea, string data)
            : base(ea)
        {
            Data = data;
        }

        public string Data { get; set; }
    }
}
