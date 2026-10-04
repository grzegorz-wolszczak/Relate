using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using static UnitTests.TestSupport.Build;

namespace UnitTests.CallProcessing;

public class CallLogMatcherTests
{
   private static readonly IReadOnlyList<RelatePhoneNumber> ContactNumbers =
      [new("123456789"), new("987654321")];

   private static readonly DateTimeOffset When = At.Utc(2026, 8, 20, 10);

   [Fact]
   public void Matches_a_call_by_normalized_number()
   {
      // Arrange
      var callLog = new[] { LogEntry("123 456 789", When, 600) };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().ContainSingle();
      result[0].CallDate.Should().Be(When);
      result[0].CallDuration.Should().Be(TimeSpan.FromSeconds(600));
   }

   [Fact]
   public void Matches_across_the_polish_country_prefix()
   {
      // Arrange
      var callLog = new[] { LogEntry("+48123456789", When, 60) };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().ContainSingle();
   }

   [Fact]
   public void Skips_entries_with_no_number()
   {
      // Arrange
      var callLog = new[] { LogEntry(null, When, 60), LogEntry("", When, 60) };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().BeEmpty();
   }

   [Fact]
   public void Excludes_calls_that_do_not_match_any_contact_number()
   {
      // Arrange
      var callLog = new[] { LogEntry("555000111", When, 60) };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().BeEmpty();
   }

   [Fact]
   public void Accumulates_all_matching_calls_in_order()
   {
      // Arrange
      var callLog = new[]
      {
         LogEntry("123456789", When, 60),
         LogEntry("555", When.AddHours(1), 60),
         LogEntry("987654321", When.AddHours(2), 120),
      };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().HaveCount(2);
      result.Select(c => c.CallDate).Should().Equal(When, When.AddHours(2));
   }

   [Fact]
   public void Contact_with_no_numbers_matches_nothing()
   {
      // Arrange
      var callLog = new[] { LogEntry("123456789", When, 60) };

      // Act
      var result = CallLogMatcher.MatchCalls([], callLog);

      // Assert
      result.Should().BeEmpty();
   }

   [Fact]
   public void A_zero_duration_call_still_produces_a_connection()
   {
      // Arrange
      var callLog = new[] { LogEntry("123456789", When, 0) };

      // Act
      var result = CallLogMatcher.MatchCalls(ContactNumbers, callLog);

      // Assert
      result.Should().ContainSingle();
      result[0].CallDuration.Should().Be(TimeSpan.Zero);
   }
}
