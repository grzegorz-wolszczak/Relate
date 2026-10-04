using System.Globalization;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Utils;

namespace Relate.AppLogic;

public static class CallDisplayHelper
{
   // The card shown for a contact's most recent contact of any type (call, meeting, video
   // call, ...). The call duration line only makes sense - and is only ever supplied - for an
   // actual phone call; other types just show when and how.
   public static string GetLastContactCardDisplay(
      DateTimeOffset? lastContactDateTime,
      ContactType? lastContactType,
      TimeSpan? lastContactCallDuration,
      DateTimeOffset now,
      TimeZoneInfo localZone)
   {
      if (lastContactDateTime is null || lastContactType is null)
      {
         return "Contact: never";
      }

      var whenHR = HumanReadableTime.DateTimeAgo(lastContactDateTime.Value, now);
      var typeLabel = ManualContactTypeCatalog.LabelFor(lastContactType.Value);

      var displayInLocalTimeZone = TimeZoneInfo.ConvertTime(lastContactDateTime.Value, localZone);
      var dateFormatted = displayInLocalTimeZone.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

      var lines = $"Contact: {whenHR} <{typeLabel}>\nat: {dateFormatted}";

      if (lastContactCallDuration is not null)
      {
         lines += $"\n(took: {HumanReadableTime.Duration(lastContactCallDuration.Value)})";
      }

      return lines;
   }

   public static string GetNextContactInDaysDisplayDetails(int nextCallInDays, bool hasLastContact)
   {
      if (!hasLastContact)
      {
         return $"next contact: today!";
      }

      var nextCallDisplay = "";

      if (nextCallInDays == 0)
      {
         nextCallDisplay = $"next contact: today!";
      }
      else if (nextCallInDays >= 0)
      {
         var nextIn = $"in {nextCallInDays}d";
         if (nextCallInDays == 1)
         {
            nextIn = "tomorrow";
         }
         nextCallDisplay = $"next contact: {nextIn}";
      }
      else // missed the contact (nextCallInDays less than zero)
      {
         nextCallDisplay = $"next contact: today! [missed: {Math.Abs(nextCallInDays)}d ago]";
      }

      return $"{nextCallDisplay}";

   }

   // date is always constructed with TimeZoneInfo.Local.GetUtcOffset(...) by the picker that
   // sets it (see ManualContactEntryVm), so .Date already is the intended local calendar date -
   // no further timezone conversion needed here.
   public static string GetManualContactDisplay(string label, DateTimeOffset? date)
   {
      if (date is null)
      {
         return $"Last {label}: <N/A>";
      }

      var formatted = date.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

      return $"Last {label}: {formatted}";
   }
}