using System.Globalization;
using Relate.Converters;

namespace UnitTests.Converters;

public class TimeSpanToStringConverterTests
{
   private readonly TimeSpanToStringConverter _sut = new();

   private object? Convert(object? value) =>
      _sut.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

   [Fact]
   public void Zero_renders_as_zero_seconds()
   {
      // Act
      var result = Convert(TimeSpan.Zero);

      // Assert
      result.Should().Be("0 sec");
   }

   [Theory]
   [InlineData(0, 0, 0, 30, "30 sec")]
   [InlineData(0, 0, 5, 0, "5 min")]
   [InlineData(0, 2, 0, 0, "2 h")]
   [InlineData(0, 1, 30, 0, "1 h 30 min")]
   [InlineData(1, 0, 0, 0, "1 day")]
   [InlineData(2, 0, 0, 0, "2 days")]
   [InlineData(1, 2, 3, 4, "1 day 2 h 3 min 4 sec")]
   public void Composes_non_zero_components(int d, int h, int m, int s, string expected)
   {
      // Arrange
      var value = new TimeSpan(d, h, m, s);

      // Act
      var result = Convert(value);

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("not a timespan")]
   [InlineData(42)]
   public void Non_timespan_input_returns_empty_string(object? value)
   {
      // Act
      var result = Convert(value);

      // Assert
      result.Should().Be(string.Empty);
   }

   [Fact]
   public void ConvertBack_is_not_supported()
   {
      // Act
      var act = () => _sut.ConvertBack("x", typeof(TimeSpan), null, CultureInfo.InvariantCulture);

      // Assert
      act.Should().Throw<NotImplementedException>();
   }
}
