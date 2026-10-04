using System.Globalization;

namespace Relate.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return FormatTimeSpan(timeSpan);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var result = string.Empty;

        if (timeSpan.Days > 0)
        {
            result += $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ";
        }

        if (timeSpan.Hours > 0)
        {
            result += $"{timeSpan.Hours} h ";
        }

        if (timeSpan.Minutes > 0)
        {
            result += $"{timeSpan.Minutes} min ";
        }

        if (timeSpan.Seconds > 0 || string.IsNullOrEmpty(result))
        {
            result += $"{timeSpan.Seconds} sec";
        }

        return result.Trim();
    }
}