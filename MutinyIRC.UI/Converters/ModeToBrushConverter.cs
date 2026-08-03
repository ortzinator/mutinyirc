using System;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Converters;

public class ModeToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mode mode)
            return LookupBrush("ModeRegularForeground", Brushes.Gray);

        return mode switch
        {
            // Owner and admin share the op colour; their '~' / '&' glyph carries the rank.
            Mode.Owner or Mode.Op => LookupBrush("ModeOpForeground", Brushes.Goldenrod),
            Mode.HalfOp => LookupBrush("ModeHalfOpForeground", Brushes.SkyBlue),
            Mode.Voice => LookupBrush("ModeVoiceForeground", Brushes.MediumSeaGreen),
            _ => LookupBrush("ModeRegularForeground", Brushes.Gray),
        };
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
