using Relate.AppLogic.CallProcessing;
using static UnitTests.TestSupport.Build;

namespace UnitTests.CallProcessing;

public class PhoneCallProcessorTests
{
   private static readonly DateTimeOffset Day1Morning = At.Utc(2026, 8, 20, 9);
   private static readonly DateTimeOffset Day1Evening = At.Utc(2026, 8, 20, 20);
   private static readonly DateTimeOffset Day2 = At.Utc(2026, 8, 21, 12);

   [Fact]
   public void Aggregate_true_merges_same_day_connections_into_one_call_with_summed_duration()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Day1Morning, 5), Call(Day1Evening, 10) };

      // Act
      var calls = PhoneCallProcessor.ProcessCallConnections(connections, shouldAggregateByDay: true)
         .GetLongCalls(TimeSpan.Zero);

      // Assert
      calls.Should().ContainSingle();
      calls[0].CallDuration.Should().Be(TimeSpan.FromMinutes(15));
      calls[0].IsAggregated.Should().BeTrue();
   }

   [Fact]
   public void Aggregate_false_keeps_each_same_day_connection_separate()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Day1Morning, 5), Call(Day1Evening, 10) };

      // Act
      var calls = PhoneCallProcessor.ProcessCallConnections(connections, shouldAggregateByDay: false)
         .GetLongCalls(TimeSpan.Zero);

      // Assert
      calls.Should().HaveCount(2);
      calls.Should().OnlyContain(c => !c.IsAggregated);
   }

   [Fact]
   public void Different_days_are_always_separate_regardless_of_aggregation()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Day1Morning, 5), Call(Day2, 10) };

      // Act
      var aggregated = PhoneCallProcessor.ProcessCallConnections(connections, true).GetLongCalls(TimeSpan.Zero);
      var separate = PhoneCallProcessor.ProcessCallConnections(connections, false).GetLongCalls(TimeSpan.Zero);

      // Assert
      aggregated.Should().HaveCount(2);
      separate.Should().HaveCount(2);
   }

   [Fact]
   public void Aggregation_changes_whether_same_day_short_calls_count_as_a_long_call()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Day1Morning, 20), Call(Day1Evening, 20) };
      var threshold = TimeSpan.FromMinutes(30);

      // Act
      var aggregated = PhoneCallProcessor.ProcessCallConnections(connections, true).GetLongCalls(threshold);
      var separate = PhoneCallProcessor.ProcessCallConnections(connections, false).GetLongCalls(threshold);

      // Assert
      aggregated.Should().ContainSingle();
      separate.Should().BeEmpty();
   }

   [Fact]
   public void Result_is_ordered_newest_first()
   {
      // Arrange
      var connections = new List<CallConnection> { Call(Day1Morning, 30), Call(Day2, 30) };

      // Act
      var calls = PhoneCallProcessor.ProcessCallConnections(connections, true).GetLongCalls(TimeSpan.Zero);

      // Assert
      calls.Should().BeInDescendingOrder(c => c.CallDateTime);
   }

   [Fact]
   public void Empty_connections_produce_no_calls()
   {
      // Arrange
      var connections = new List<CallConnection>();

      // Act
      var result = PhoneCallProcessor.ProcessCallConnections(connections, shouldAggregateByDay: true);

      // Assert
      result.GetLongCalls(TimeSpan.Zero).Should().BeEmpty();
   }
}
