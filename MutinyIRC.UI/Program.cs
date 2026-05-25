using global::Avalonia;
using System;
using System.Diagnostics;
using MutinyIRC.Common;

namespace MutinyIRC.UI;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        foreach (string nick in AppSettings.Instance.ServiceNicks)
            Server.ServiceNicks.Add(nick);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
