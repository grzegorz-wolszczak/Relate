using Relate.AppLogic.CallProcessing;
using static UnitTests.TestSupport.Build;

namespace UnitTests.CallProcessing;

public class CallScheduleCalculatorTests
{
   private readonly CallScheduleCalculator _sut = new();
   private static readonly DateTimeOffset Now = At.Utc(2026, 8, 27, 10);

   private static PhoneCalls Calls(params (DateTimeOffset day, double minutes)[] calls) =>
      PhoneCallProcessor.ProcessCallConnections(
         calls.Select(c => Call(c.day, c.minutes)).ToList(), shouldAggregateByDay: true);

   private static readonly List<(ContactType Type, DateTimeOffset? Date)> NoManualDates = [];

   private NextCallRecalculation Recalc(
      PhoneCalls calls, int longCallMinutes = 30, int noContactPeriodDays = 30,
      IReadOnlyList<(ContactType Type, DateTimeOffset? Date)>? manualDates = null) =>
      _sut.RecalculateCallsRelatedData(calls, longCallMinutes, noContactPeriodDays, manualDates ?? NoManualDates, Now);

   [Fact]
   public void No_long_calls_yields_zero_days_and_today_at_1700()
   {
      // Arrange
      var calls = Calls();

      // Act
      var result = Recalc(calls);

      // Assert
      result.NextCallInDays.Should().Be(0);
      result.NextCallDateTime.Date.Should().Be(Now.Date);
      result.NextCallDateTime.TimeOfDay.Should().Be(TimeSpan.FromHours(17));
   }

   [Fact]
   public void Short_call_below_threshold_is_not_a_long_call()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-1), 20));

      // Act
      var result = Recalc(calls, longCallMinutes: 30);

      // Assert
      result.NextCallInDays.Should().Be(0);
   }

   [Fact]
   public void Call_today_schedules_the_full_period_ahead()
   {
      // Arrange
      var calls = Calls((Now, 45));

      // Act
      var result = Recalc(calls, noContactPeriodDays:30);

      // Assert
      result.NextCallInDays.Should().Be(30);
      result.NextCallDateTime.Date.Should().Be(Now.Date.AddDays(30));
      result.NextCallDateTime.TimeOfDay.Should().Be(TimeSpan.FromHours(17));
   }

   [Fact]
   public void Partway_through_the_period_returns_the_remaining_days()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-10), 45));

      // Act
      var result = Recalc(calls, noContactPeriodDays:30);

      // Assert
      result.NextCallInDays.Should().Be(20);
      result.NextCallDateTime.Date.Should().Be(Now.Date.AddDays(20));
   }

   [Fact]
   public void Exactly_due_today_returns_zero_and_does_not_shift_the_date()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-30), 45));

      // Act
      var result = Recalc(calls, noContactPeriodDays:30);

      // Assert
      result.NextCallInDays.Should().Be(0);
      result.NextCallDateTime.Date.Should().Be(Now.Date);
   }

   [Fact]
   public void Overdue_call_returns_negative_days_and_today_at_1700()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-40), 45));

      // Act
      var result = Recalc(calls, noContactPeriodDays:30);

      // Assert
      result.NextCallInDays.Should().Be(-10);
      result.NextCallDateTime.Date.Should().Be(Now.Date);
      result.NextCallDateTime.TimeOfDay.Should().Be(TimeSpan.FromHours(17));
   }

   [Fact]
   public void Time_of_day_is_ignored_only_the_calendar_date_counts()
   {
      // Arrange
      var lastCall = At.Utc(2026, 8, 26, 23, 30); // yesterday late
      var now = At.Utc(2026, 8, 27, 1, 0);        // today early
      var calls = Calls((lastCall, 45));

      // Act
      var result = _sut.RecalculateCallsRelatedData(calls, longCallDurationMinutes: 30, noContactPeriodDays: 1, NoManualDates, now);

      // Assert
      result.NextCallInDays.Should().Be(0);
   }

   [Fact]
   public void Zero_period_is_due_the_same_calendar_day_as_the_last_call()
   {
      // Arrange
      var today = Calls((Now, 45));
      var fiveDaysAgo = Calls((Now.AddDays(-5), 45));

      // Act
      var dueToday = Recalc(today, noContactPeriodDays:0);
      var overdue = Recalc(fiveDaysAgo, noContactPeriodDays:0);

      // Assert
      dueToday.NextCallInDays.Should().Be(0);
      overdue.NextCallInDays.Should().Be(-5);
   }

   [Fact]
   public void Uses_the_most_recent_long_call()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-100), 45), (Now.AddDays(-5), 45));

      // Act
      var result = Recalc(calls, noContactPeriodDays:30);

      // Assert
      result.NextCallInDays.Should().Be(25);
   }

   [Fact]
   public void Negative_offset_now_is_handled_by_date_only_math()
   {
      // Arrange
      var now = new DateTimeOffset(2026, 8, 27, 2, 0, 0, TimeSpan.FromHours(-8));
      var calls = Calls((now.AddDays(-10), 45));

      // Act
      var result = _sut.RecalculateCallsRelatedData(calls, longCallDurationMinutes: 30, noContactPeriodDays: 30, NoManualDates, now);

      // Assert
      result.NextCallInDays.Should().Be(20);
   }

   [Fact]
   public void Meeting_more_recent_than_the_last_long_call_wins()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-20), 45));
      var manualDates = new List<(ContactType, DateTimeOffset?)> {(ContactType.Meeting, Now.AddDays(-2))};

      // Act
      var result = Recalc(calls, noContactPeriodDays: 30, manualDates: manualDates);

      // Assert
      result.NextCallInDays.Should().Be(28);
      result.LastContactDateTime!.Value.Date.Should().Be(Now.AddDays(-2).Date);
      result.LastContactType.Should().Be(ContactType.Meeting);
   }

   [Fact]
   public void Long_call_more_recent_than_a_meeting_wins()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-2), 45));
      var manualDates = new List<(ContactType, DateTimeOffset?)> {(ContactType.Meeting, Now.AddDays(-20))};

      // Act
      var result = Recalc(calls, noContactPeriodDays: 30, manualDates: manualDates);

      // Assert
      result.NextCallInDays.Should().Be(28);
      result.LastContactDateTime!.Value.Date.Should().Be(Now.AddDays(-2).Date);
      result.LastContactType.Should().Be(ContactType.Call);
      result.LastContactCallDuration.Should().Be(TimeSpan.FromMinutes(45));
   }

   [Fact]
   public void A_meeting_winning_over_a_call_reports_no_call_duration()
   {
      // Arrange
      var calls = Calls((Now.AddDays(-20), 45));
      var manualDates = new List<(ContactType, DateTimeOffset?)> {(ContactType.Meeting, Now.AddDays(-2))};

      // Act
      var result = Recalc(calls, noContactPeriodDays: 30, manualDates: manualDates);

      // Assert
      result.LastContactType.Should().Be(ContactType.Meeting);
      result.LastContactCallDuration.Should().BeNull();
   }

   [Fact]
   public void No_calls_but_a_meeting_is_recorded_uses_the_meeting()
   {
      // Arrange
      var manualDates = new List<(ContactType, DateTimeOffset?)> {(ContactType.Meeting, Now.AddDays(-10))};

      // Act
      var result = Recalc(Calls(), noContactPeriodDays: 30, manualDates: manualDates);

      // Assert
      result.NextCallInDays.Should().Be(20);
      result.LastContactType.Should().Be(ContactType.Meeting);
   }

   [Fact]
   public void Most_recent_of_two_manual_types_wins()
   {
      // Arrange
      var manualDates = new List<(ContactType, DateTimeOffset?)>
      {
         (ContactType.Meeting, Now.AddDays(-15)),
         (ContactType.VideoCall, Now.AddDays(-3)),
      };

      // Act
      var result = Recalc(Calls(), noContactPeriodDays: 30, manualDates: manualDates);

      // Assert
      result.NextCallInDays.Should().Be(27);
      result.LastContactType.Should().Be(ContactType.VideoCall);
   }

   [Fact]
   public void No_calls_and_no_manual_dates_yields_null_last_contact()
   {
      // Act
      var result = Recalc(Calls(), noContactPeriodDays: 30);

      // Assert
      result.NextCallInDays.Should().Be(0);
      result.LastContactDateTime.Should().BeNull();
      result.LastContactType.Should().BeNull();
   }
}
