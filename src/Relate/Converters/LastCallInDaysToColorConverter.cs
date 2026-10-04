using System.Globalization;

namespace Relate.Converters;

public class LastCallInDaysToColorConverter : IValueConverter
{
   // Soft Pastel urgency tokens - see UrgencyRed / UrgencyAmber / UrgencyGreen in Resources/Styles/Colors.xaml.
   public static readonly Color UrgencyRed = Color.FromArgb("#E1544B");
   public static readonly Color UrgencyAmber = Color.FromArgb("#E08A2B");
   public static readonly Color UrgencyGreen = Color.FromArgb("#3FA772");

   private readonly int _lessThanDaysOrange;
   private readonly int _lessThanDaysRed;

   public LastCallInDaysToColorConverter(int lessThanDaysOrange = 14, int lessThanDaysRed = 7)
   {
      _lessThanDaysOrange = lessThanDaysOrange;
      _lessThanDaysRed = lessThanDaysRed;
   }

   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is int nextCalInDays)
      {
         return NextCallInDaysAsColor(nextCalInDays);
      }


      return Colors.Black;
   }

   private object NextCallInDaysAsColor(int nextCallInDays)
   {
      if (nextCallInDays <= _lessThanDaysRed)
      {
         return UrgencyRed;
      }

      if (nextCallInDays < _lessThanDaysOrange)
      {
         return UrgencyAmber;
      }

      return UrgencyGreen;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
      throw new NotImplementedException();


}