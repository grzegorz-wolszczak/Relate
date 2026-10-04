using System.Globalization;
using Relate.Converters;

namespace UnitTests.Converters;

public class LastCallInDaysToColorConverterTests
{
   private static object? Convert(object? value, LastCallInDaysToColorConverter? converter = null) =>
      (converter ?? new LastCallInDaysToColorConverter())
         .Convert(value, typeof(Color), null, CultureInfo.InvariantCulture);

   [Theory]
   [InlineData(-5)]
   [InlineData(0)]
   [InlineData(7)]
   public void At_or_below_the_red_threshold_is_red(int days)
   {
      // Act
      var result = Convert(days);

      // Assert
      result.Should().Be(LastCallInDaysToColorConverter.UrgencyRed);
   }

   [Theory]
   [InlineData(8)]
   [InlineData(13)]
   public void Between_the_thresholds_is_amber(int days)
   {
      // Act
      var result = Convert(days);

      // Assert
      result.Should().Be(LastCallInDaysToColorConverter.UrgencyAmber);
   }

   [Theory]
   [InlineData(14)]
   [InlineData(30)]
   public void At_or_above_the_orange_threshold_is_green(int days)
   {
      // Act
      var result = Convert(days);

      // Assert
      result.Should().Be(LastCallInDaysToColorConverter.UrgencyGreen);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("x")]
   [InlineData(3.5)]
   public void Non_int_input_is_black(object? value)
   {
      // Act
      var result = Convert(value);

      // Assert
      result.Should().Be(Colors.Black);
   }

   [Fact]
   public void Custom_thresholds_are_honoured()
   {
      // Arrange
      var converter = new LastCallInDaysToColorConverter(lessThanDaysOrange: 60, lessThanDaysRed: 30);

      // Act
      var red = Convert(30, converter);
      var amber = Convert(45, converter);
      var green = Convert(60, converter);

      // Assert
      red.Should().Be(LastCallInDaysToColorConverter.UrgencyRed);
      amber.Should().Be(LastCallInDaysToColorConverter.UrgencyAmber);
      green.Should().Be(LastCallInDaysToColorConverter.UrgencyGreen);
   }

   [Fact]
   public void ConvertBack_is_not_supported()
   {
      // Act
      var act = () => new LastCallInDaysToColorConverter()
         .ConvertBack(Colors.Red, typeof(int), null, CultureInfo.InvariantCulture);

      // Assert
      act.Should().Throw<NotImplementedException>();
   }
}
