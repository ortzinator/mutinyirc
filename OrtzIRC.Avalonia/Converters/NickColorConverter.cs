namespace OrtzIRC.Avalonia.Converters;

using System;
using System.Globalization;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;

public class NickColorConverter : IValueConverter
{
    private static readonly IBrush[] Palette =
    {
        new SolidColorBrush(Color.FromRgb(0x7B, 0x8D, 0xFF)), // indigo-blue
        new SolidColorBrush(Color.FromRgb(0x57, 0xCC, 0x77)), // green
        new SolidColorBrush(Color.FromRgb(0xE0, 0xA8, 0x3A)), // amber
        new SolidColorBrush(Color.FromRgb(0xE0, 0x65, 0x65)), // red
        new SolidColorBrush(Color.FromRgb(0xCC, 0x55, 0x9E)), // pink
        new SolidColorBrush(Color.FromRgb(0x30, 0xBB, 0xE8)), // cyan
        new SolidColorBrush(Color.FromRgb(0xE0, 0x89, 0x30)), // orange
        new SolidColorBrush(Color.FromRgb(0x9B, 0x6F, 0xE8)), // purple
        new SolidColorBrush(Color.FromRgb(0x2A, 0xC5, 0xA8)), // teal
        new SolidColorBrush(Color.FromRgb(0xE8, 0x7A, 0x56)), // coral
        new SolidColorBrush(Color.FromRgb(0x5B, 0xB8, 0xD4)), // sky
        new SolidColorBrush(Color.FromRgb(0xD4, 0xA0, 0x5B)), // gold
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string nick || string.IsNullOrEmpty(nick))
            return Brushes.Gray;

        int hash = 0;
        foreach (char c in nick)
            hash = hash * 31 + c;

        return Palette[Math.Abs(hash) % Palette.Length];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
