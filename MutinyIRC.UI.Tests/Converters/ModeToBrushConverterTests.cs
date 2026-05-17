using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using MutinyIRC.UI.Converters;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.Converters;

/// <summary>
/// Regression tests for ModeToBrushConverter not receiving the theme variant.
///
/// The bug: LookupBrush passed null (or the wrong variant) to TryGetResource, so the
/// resource lookup always missed and fell back to the hardcoded fallback brush, meaning
/// Op nicks looked the same in both Dark and Light themes.
///
/// The fix: use Application.Current?.ActualThemeVariant which reflects the live theme.
/// </summary>
[TestFixture]
public class ModeToBrushConverterTests
{
    private static readonly ModeToBrushConverter Converter = new();
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // --- Fallback / type-safety tests (no Application required) ---

    [Test]
    public void Convert_NullValue_ReturnsIBrush()
    {
        var result = Converter.Convert(null!, typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    [Test]
    public void Convert_WrongType_ReturnsIBrush()
    {
        var result = Converter.Convert("not-a-mode", typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    [Test]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            Converter.ConvertBack(Brushes.Black, typeof(Mode), null!, Culture));
    }

    // --- Tests requiring the headless Application (theme resource lookups) ---

    [AvaloniaTest]
    public void Convert_OpMode_ReturnsBrush()
    {
        var result = Converter.Convert(Mode.Op, typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    [AvaloniaTest]
    public void Convert_VoiceMode_ReturnsBrush()
    {
        var result = Converter.Convert(Mode.Voice, typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    [AvaloniaTest]
    public void Convert_RegularMode_ReturnsBrush()
    {
        var result = Converter.Convert(Mode.Regular, typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    [AvaloniaTest]
    public void Convert_OwnerMode_ReturnsBrush()
    {
        var result = Converter.Convert(Mode.Owner, typeof(IBrush), null!, Culture);
        Assert.That(result, Is.InstanceOf<IBrush>());
    }

    /// <summary>
    /// Core theme-variant regression: Op brush in Dark theme must differ from Light theme
    /// because DefaultTheme.axaml defines different colors for ModeOpForeground in each
    /// ThemeDictionary entry.  If ActualThemeVariant is null (regression), TryGetResource
    /// misses and both return the same fallback Brushes.Black.
    /// </summary>
    [AvaloniaTest]
    public void Convert_OpMode_DarkAndLightThemeReturnDifferentBrushes()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var darkBrush = (IBrush)Converter.Convert(Mode.Op, typeof(IBrush), null!, Culture);

        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var lightBrush = (IBrush)Converter.Convert(Mode.Op, typeof(IBrush), null!, Culture);

        // Dark: ModeOpForeground = #FFFFFFFF (white)
        // Light: ModeOpForeground = #FF8B6200 (gold/amber)
        // They must not be equal — if they are, the theme variant lookup is broken.
        Assert.That(darkBrush.ToString(), Is.Not.EqualTo(lightBrush.ToString()),
            "Op brush must differ between Dark and Light themes; " +
            "equal values indicate ActualThemeVariant is not being passed to TryGetResource");
    }

    [AvaloniaTest]
    public void Convert_VoiceMode_DarkAndLightThemeReturnDifferentBrushes()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var darkBrush = (IBrush)Converter.Convert(Mode.Voice, typeof(IBrush), null!, Culture);

        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var lightBrush = (IBrush)Converter.Convert(Mode.Voice, typeof(IBrush), null!, Culture);

        Assert.That(darkBrush.ToString(), Is.Not.EqualTo(lightBrush.ToString()),
            "Voice brush must differ between themes");
    }

    /// <summary>
    /// With the correct theme, the converter must return a theme resource brush
    /// rather than the hardcoded fallback.  Dark theme ModeOpForeground is white (#FF),
    /// which is NOT the fallback Brushes.Black.
    /// </summary>
    [AvaloniaTest]
    public void Convert_OpMode_DarkTheme_ReturnsThemeResourceNotFallback()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var brush = (IBrush)Converter.Convert(Mode.Op, typeof(IBrush), null!, Culture);

        // If LookupBrush returned the fallback Brushes.Black the color would be #FF000000.
        // The Dark theme resource is #FFFFFFFF (white), so they must differ.
        Assert.That(brush, Is.Not.SameAs(Brushes.Black),
            "Dark theme Op brush must be the themed resource, not the hardcoded fallback Black");
    }

    [AvaloniaTest]
    public void Convert_OpMode_LightTheme_ReturnsThemeResourceNotFallback()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var brush = (IBrush)Converter.Convert(Mode.Op, typeof(IBrush), null!, Culture);

        // Light theme ModeOpForeground is #FF8B6200 (gold), not Black.
        Assert.That(brush, Is.Not.SameAs(Brushes.Black),
            "Light theme Op brush must be the themed resource, not the hardcoded fallback Black");
    }
}
