using global::Avalonia;
using System;
using System.Diagnostics;

namespace MutinyIRC.UI;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
