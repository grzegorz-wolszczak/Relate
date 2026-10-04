using Relate.AppLogic.Utils;

namespace UnitTests.Utils;

public class HumanReadableTimeAgoTests
{
   private static readonly DateTimeOffset Now = At.Utc(2026, 8, 27, 12);

   private static string Ago(TimeSpan since) => HumanReadableTime.DateTimeAgo(Now - since, Now);

   [Theory]
   [InlineData(1, "1s ago")]
   [InlineData(5, "5s ago")]
   public void Seconds(int seconds, string expected)
   {
      // Act
      var result = Ago(TimeSpan.FromSeconds(seconds));

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(1, "1min ago")]
   [InlineData(45, "45min ago")]
   public void Minutes(int minutes, string expected)
   {
      // Act
      var result = Ago(TimeSpan.FromMinutes(minutes));

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(1, "1h ago")]
   [InlineData(5, "5h ago")]
   public void Hours(int hours, string expected)
   {
      // Act
      var result = Ago(TimeSpan.FromHours(hours));

      // Assert
      result.Should().Be(expected);
   }

   [Theory]
   [InlineData(1, "1d ago")]
   [InlineData(15, "15d ago")]
   public void Days(int days, string expected)
   {
      // Act
      var result = Ago(TimeSpan.FromDays(days));

      // Assert
      result.Should().Be(expected);
   }

   [Fact]
   public void Months_and_remaining_days()
   {
      // Act
      var result = Ago(TimeSpan.FromDays(65));

      // Assert
      result.Should().Be("2m5d ago");
   }

   [Fact]
   public void Years_months_and_days()
   {
      // Act
      var result = Ago(TimeSpan.FromDays(400));

      // Assert
      result.Should().Be("1y1m5d ago");
   }

   [Fact]
   public void The_parameterless_overload_uses_the_wall_clock()
   {
      // Arrange
      var twoSecondsAgo = DateTimeOffset.Now.AddSeconds(-2);

      // Act
      var result = HumanReadableTime.DateTimeAgo(twoSecondsAgo);

      // Assert
      result.Should().EndWith("ago");
   }
}