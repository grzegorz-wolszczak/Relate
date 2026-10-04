namespace Relate.AppLogic.CallProcessing;

public class CallScheduleCalculator
{

   // if return value less than zero that it means that we missed this contact by that many days and should reach out now

   // also: ignore time , use only Dates
   private static int HowManyDaysToNextCall(DateTimeOffset lastContactDate, int noContactPeriodDays, DateTimeOffset nowDateTimeOffset)
   {
      var now = nowDateTimeOffset.Date;
      var nextCallDate = lastContactDate.Date.AddDays(noContactPeriodDays);

      var timeLeft = nextCallDate - now;
      return timeLeft.Days;
   }

   // picks whichever candidate happened most recently; candidates with a null date are ignored.
   private static (DateTimeOffset? DateTime, ContactType? Type) DetermineLastContact(
      params (DateTimeOffset? Date, ContactType Type)[] candidates)
   {
      foreach (var candidate in candidates.Where(c => c.Date is not null).OrderByDescending(c => c.Date))
      {
         return (candidate.Date, candidate.Type);
      }

      return (null, null);
   }

   public NextCallRecalculation RecalculateCallsRelatedData(PhoneCalls relateContactCalls,
      int longCallDurationMinutes,
      int noContactPeriodDays,
      IReadOnlyList<(ContactType Type, DateTimeOffset? Date)> manualContactDates,
      DateTimeOffset localNow)
   {
      var properCallDuration = TimeSpan.FromMinutes(longCallDurationMinutes);

      var longCalls = relateContactCalls.GetLongCalls(properCallDuration);

      var lastLongCall = longCalls.FirstOrDefault();

      var candidates = manualContactDates
         .Select(m => (m.Date, m.Type))
         .Append((lastLongCall?.CallDateTime, ContactType.Call))
         .ToArray();

      var (lastContactDateTime, lastContactType) = DetermineLastContact(candidates);

      // the "(took: ...)" duration only makes sense for an actual phone call
      var lastContactCallDuration = lastContactType == ContactType.Call ? lastLongCall?.CallDuration : null;

      // pretend that we always reach out at 17:00
      var nextCallDateTime = localNow.Date.AddHours(17);
      if (lastContactDateTime is null)
      {
         return new()
         {
            NextCallDateTime = nextCallDateTime,
            NextCallInDays = 0,
            LastContactDateTime = null,
            LastContactType = null,
            LastContactCallDuration = null
         };
      }

      var nextCallInDays = HowManyDaysToNextCall(lastContactDateTime.Value, noContactPeriodDays, localNow);

      if (nextCallInDays <= 0)
      {
         return new()
         {
            NextCallDateTime = nextCallDateTime,
            NextCallInDays = nextCallInDays,
            LastContactDateTime = lastContactDateTime,
            LastContactType = lastContactType,
            LastContactCallDuration = lastContactCallDuration
         };
      }

      return new()
      {
         NextCallDateTime = nextCallDateTime + TimeSpan.FromDays(nextCallInDays),
         NextCallInDays = nextCallInDays,
         LastContactDateTime = lastContactDateTime,
         LastContactType = lastContactType,
         LastContactCallDuration = lastContactCallDuration
      };
   }
}
