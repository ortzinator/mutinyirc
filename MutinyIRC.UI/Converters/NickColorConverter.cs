using System;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;

namespace MutinyIRC.UI.Converters;

/// <summary>
/// Gives every nick a stable colour by hashing it into a ten-hue palette. The palette lives in the
/// theme dictionaries as <c>NickPalette0</c>..<c>NickPalette9</c>, so light and dark each get hues
/// with the right contrast against their own background while a nick keeps its slot in both.
/// </summary>
public class NickColorConverter : IValueConverter
{
    private const int PaletteSize = 10;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string nick || string.IsNullOrEmpty(nick))
            return LookupBrush("TextPrimary", Brushes.Gray);

        return LookupBrush("NickPalette" + SlotFor(nick), Brushes.Gray);
    }

    /// <summary>
    /// Which palette slot a nick lands in. The modulo keeps the running hash inside int range, so
    /// the slot depends only on the nick text — never on how long it happens to be.
    /// </summary>
    public static int SlotFor(string nick)
    {
        int hash = 0;
        foreach (char c in nick)
            hash = (hash * 31 + c) % 997;

        return hash % PaletteSize;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static IBrush LookupBrush(string key, IBrush fallback)
    {
        var theme = Application.Current?.ActualThemeVariant;
        if (Application.Current?.Resources.TryGetResource(key, theme, out var resource) == true
            && resource is IBrush brush)
            return brush;

        return fallback;
    }
}
