using Relate.AppLogic.CallProcessing;
using static UnitTests.TestSupport.Build;

namespace UnitTests.CallProcessing;

public class PhoneCallTests
{
   private static readonly DateTimeOffset Morning = At.Utc(2026, 8, 20, 9);
   private static readonly DateTimeOffset Noon = At.Utc(2026, 8, 20, 12);
   private static readonly DateTimeOffset Evening = At.Utc(2026, 8, 20, 21);

   [Fact]
   public void Empty_connection_list_throws()
   {
      // Arrange
      var connections = new List<CallConnection>();

      // Act
      var act = () => new PhoneCall(connections);

      // Assert
      act.Should().Throw<InvalidOperationException>().WithMessage("call connections are empty");
   }

   [Fact]
   public void Connections_spanning_more_than_one_day_throw()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Morning, 5), Call(At.Utc(2026, 8, 21, 9), 5) };

      // Act
      var act = () => new PhoneCall(connections);

      // Assert
      act.Should().Throw<InvalidOperationException>().WithMessage("not all calls are on the same day");
   }

   [Fact]
   public void Duration_is_the_sum_of_all_connection_durations()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Morning, 5), Call(Noon, 10), Call(Evening, 15) };

      // Act
      var call = new PhoneCall(connections);

      // Assert
      call.CallDuration.Should().Be(TimeSpan.FromMinutes(30));
   }

   [Fact]
   public void CallDateTime_is_the_oldest_connection()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Evening, 5), Call(Morning, 5), Call(Noon, 5) };

      // Act
      var call = new PhoneCall(connections);

      // Assert
      call.CallDateTime.Should().Be(Morning);
   }

   [Fact]
   public void IsAggregated_is_true_only_with_more_than_one_connection()
   {
      // Arrange
      var single = new List<CallConnection> { Call(Morning, 5) };
      var multiple = new List<CallConnection> { Call(Morning, 5), Call(Noon, 5) };

      // Act
      var singleCall = new PhoneCall(single);
      var aggregatedCall = new PhoneCall(multiple);

      // Assert
      singleCall.IsAggregated.Should().BeFalse();
      aggregatedCall.IsAggregated.Should().BeTrue();
   }

   [Fact]
   public void Connections_are_stored_newest_first()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Morning, 5), Call(Evening, 5), Call(Noon, 5) };

      // Act
      var call = new PhoneCall(connections);

      // Assert
      call.CallConnections.Should().BeInDescendingOrder(c => c.CallDate);
   }

   [Fact]
   public void Single_connection_passes_its_values_through()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Noon, 7) };

      // Act
      var call = new PhoneCall(connections);

      // Assert
      call.CallDateTime.Should().Be(Noon);
      call.CallDuration.Should().Be(TimeSpan.FromMinutes(7));
   }
}
