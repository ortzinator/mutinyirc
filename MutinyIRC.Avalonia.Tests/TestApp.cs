using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(MutinyIRC.Avalonia.Tests.TestApp))]

namespace MutinyIRC.Avalonia.Tests;

public class TestApp : global::Avalonia.Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Resources.MergedDictionaries.Add(
            new global::Avalonia.Markup.Xaml.Styling.ResourceInclude(
                new Uri("avares://OrtzIRC.Avalonia/Themes/DefaultTheme.axaml"))
            {
                Source = new Uri("avares://OrtzIRC.Avalonia/Themes/DefaultTheme.axaml")
            });
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>();
}
