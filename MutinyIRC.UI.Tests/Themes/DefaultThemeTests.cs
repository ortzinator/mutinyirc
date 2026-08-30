using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using NUnit.Framework;

namespace MutinyIRC.UI.Tests.Themes;

/// <summary>
/// Pins the brushes the Networks dialog introduced. A view may only draw with a brush the theme
/// defines in both variants: a key that exists in one and not the other is invisible in the
/// other, and the failure shows up only as a blank region on a screenshot.
/// </summary>
[TestFixture]
public class DefaultThemeTests
{
    private static Color Lookup(string key, ThemeVariant variant)
    {
        Assert.That(Application.Current!.TryGetResource(key, variant, out object? found), Is.True,
            $"'{key}' has to be defined for the {variant} variant");
        Assert.That(found, Is.InstanceOf<ISolidColorBrush>(), $"'{key}' has to be a brush");
        return ((ISolidColorBrush)found!).Color;
    }

    [AvaloniaTest]
    [TestCase("SurfaceCard")]
    [TestCase("StatusSuccessForeground")]
    public void ANewBrush_IsDefinedInBothVariants(string key)
    {
        Color dark = Lookup(key, ThemeVariant.Dark);
        Color light = Lookup(key, ThemeVariant.Light);

        Assert.That(dark, Is.Not.EqualTo(light),
            $"'{key}' reading the same in both variants means one of them was never tuned");
    }

    [AvaloniaTest]
    public void SurfaceCard_SitsAboveTheSurfaceItRestsOn()
    {
        // A card is drawn on a Layer surface. Sharing that surface's colour would leave the
        // card outline as the only thing separating them.
        Assert.That(Lookup("SurfaceCard", ThemeVariant.Dark),
            Is.Not.EqualTo(Lookup("SurfaceLayer", ThemeVariant.Dark)));
    }

    [AvaloniaTest]
    public void StatusSuccessForeground_IsNotANickHue()
    {
        // HueGreen is a nick colour and says nothing about state, so the TLS badge must not
        // borrow it — the two would then have to change together.
        Application.Current!.TryGetResource("HueGreen", ThemeVariant.Dark, out object? nickHue);

        Assert.That(Lookup("StatusSuccessForeground", ThemeVariant.Dark),
            Is.Not.EqualTo(nickHue));
    }
}
