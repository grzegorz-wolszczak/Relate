using Relate.AppLogic.CallProcessing;
using static UnitTests.TestSupport.Build;

namespace UnitTests.CallProcessing;

public class PhoneCallsTests
{
   private static PhoneCall CallOn(DateTimeOffset day, double minutes) =>
      new(new List<CallConnection> { Call(day, minutes) });

   [Fact]
   public void GetLongCalls_returns_calls_at_or_above_the_threshold()
   {
      // Arrange
      var calls = new PhoneCalls(new List<PhoneCall>
      {
         CallOn(At.Utc(2026, 8, 20), 29),
         CallOn(At.Utc(2026, 8, 21), 30),
         CallOn(At.Utc(2026, 8, 22), 31),
      });

      // Act
      var result = calls.GetLongCalls(TimeSpan.FromMinutes(30));

      // Assert
      result.Should().HaveCount(2);
      result.Should().OnlyContain(c => c.CallDuration >= TimeSpan.FromMinutes(30));
   }

   [Fact]
   public void GetLongCalls_orders_newest_first()
   {
      // Arrange
      var calls = new PhoneCalls(new List<PhoneCall>
      {
         CallOn(At.Utc(2026, 8, 20), 40),
         CallOn(At.Utc(2026, 8, 25), 40),
         CallOn(At.Utc(2026, 8, 22), 40),
      });

      // Act
      var result = calls.GetLongCalls(TimeSpan.FromMinutes(10));

      // Assert
      result.Should().BeInDescendingOrder(c => c.CallDateTime);
   }

   [Fact]
   public void GetLongCalls_with_zero_threshold_returns_all()
   {
      // Arrange
      var calls = new PhoneCalls(new List<PhoneCall> { CallOn(At.Utc(2026, 8, 20), 1), CallOn(At.Utc(2026, 8, 21), 2) });

      // Act
      var result = calls.GetLongCalls(TimeSpan.Zero);

      // Assert
      result.Should().HaveCount(2);
   }

   [Fact]
   public void GetLongCalls_returns_empty_when_nothing_qualifies()
   {
      // Arrange
      var calls = new PhoneCalls(new List<PhoneCall> { CallOn(At.Utc(2026, 8, 20), 5) });

      // Act
      var result = calls.GetLongCalls(TimeSpan.FromMinutes(10));

      // Assert
      result.Should().BeEmpty();
   }

   [Fact]
   public void GetLongCalls_on_empty_set_returns_empty()
   {
      // Arrange
      var calls = new PhoneCalls(new List<PhoneCall>());

      // Act
      var result = calls.GetLongCalls(TimeSpan.Zero);

      // Assert
      result.Should().BeEmpty();
   }
}
