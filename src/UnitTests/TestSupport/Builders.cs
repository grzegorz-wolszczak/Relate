using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Models;
using Relate.AppLogic.Services.ConditionalCompilation;

namespace UnitTests.TestSupport;

public static class Build
{
   /// <summary>A single call connection on <paramref name="when"/> lasting <paramref name="minutes"/> minutes.</summary>
   public static CallConnection Call(DateTimeOffset when, double minutes) => new()
   {
      CallDate = when,
      CallDuration = TimeSpan.FromMinutes(minutes),
   };

   public static CallConnection CallSeconds(DateTimeOffset when, long seconds) => new()
   {
      CallDate = when,
      CallDuration = TimeSpan.FromSeconds(seconds),
   };

   public static CallLogEntry LogEntry(string? number, DateTimeOffset date, long durationSeconds, string? name = null) => new()
   {
      Number = number,
      Date = date,
      DurationSeconds = durationSeconds,
      PersonWhoCalledName = name,
   };

   public static MySystemContact SystemContact(string id, string displayName, params string[] phones)
      => new(new AndroidContact
      {
         Id = id, DisplayName = displayName,
         Phones = phones.Select(p => new AndroidPhoneNumber(p, "")).ToArray()
      });
}

/// <summary>Fixed instants used across tests. All UTC so date-only math is unambiguous.</summary>
public static class At
{
   public static DateTimeOffset Utc(int year, int month, int day, int hour = 12, int minute = 0)
      => new(year, month, day, hour, minute, 0, TimeSpan.Zero);
}
