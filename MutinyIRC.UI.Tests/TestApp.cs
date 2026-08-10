using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(MutinyIRC.UI.Tests.TestApp))]

namespace MutinyIRC.UI.Tests;

public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Resources.MergedDictionaries.Add(
            new ResourceInclude(
                new Uri("avares://MutinyIRC.UI/Themes/DefaultTheme.axaml"))
            {
                Source = new Uri("avares://MutinyIRC.UI/Themes/DefaultTheme.axaml")
            });
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>();
}
