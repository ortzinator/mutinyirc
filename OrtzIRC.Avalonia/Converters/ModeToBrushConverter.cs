namespace OrtzIRC.Avalonia.Converters;

using System;
using System.Globalization;
using global::Avalonia;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;
using OrtzIRC.Avalonia.ViewModels;

public class ModeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Mode mode)
            return LookupBrush("ModeRegularForeground", Brushes.Gray);

        return mode switch
        {
            Mode.Op => LookupBrush("ModeOpForeground", Brushes.Black),
            Mode.Voice => LookupBrush("ModeVoiceForeground", Brushes.DarkOrange),
            _ => LookupBrush("ModeRegularForeground", Brushes.Gray),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
