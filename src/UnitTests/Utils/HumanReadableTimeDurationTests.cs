using Relate.AppLogic.Utils;

namespace UnitTests.Utils;

public class HumanReadableTimeDurationTests
{
   [Fact]
   public void Zero_is_zero_seconds()
   {
      // Act
      var result = HumanReadableTime.Duration(TimeSpan.Zero);

      // Assert
      result.Should().Be("0s");
   }

   [Theory]
   [InlineData(45, "45s")]
   [InlineData(59, "59s")]
   public void Under_a_minute_is_seconds(int seconds, string expected)
   {
      // Act
      var result = HumanReadableTime.Duration(TimeSpan.FromSeconds(seconds));

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(60, "1min")]
   [InlineData(90, "1m30s")]
   [InlineData(59 * 60 + 59, "59m59s")]
   public void Under_an_hour_is_minutes_and_optional_seconds(int seconds, string expected)
   {
      // Act
      var result = HumanReadableTime.Duration(TimeSpan.FromSeconds(seconds));

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(60, "1h")]
   [InlineData(90, "1h30m")]
   [InlineData(150, "2h30m")]
   public void An_hour_or_more_is_hours_and_optional_minutes(int minutes, string expected)
   {
      // Act
      var result = HumanReadableTime.Duration(TimeSpan.FromMinutes(minutes));

      // Assert
      result.Should().Be(expected);
   }
}