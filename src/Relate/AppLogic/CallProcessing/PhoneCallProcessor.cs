namespace Relate.AppLogic.CallProcessing;

public static class PhoneCallProcessor
{
   public static PhoneCalls ProcessCallConnections(List<CallConnection> connections, bool shouldAggregateByDay)
   {
      // aggregated: one PhoneCall per calendar day (durations summed).
      // not aggregated: one PhoneCall per individual call connection.
      IEnumerable<IEnumerable<CallConnection>> groups = shouldAggregateByDay
         ? connections.GroupBy(c => DateOnly.FromDateTime(c.CallDate.Date))
         : connections.Select(c => new List<CallConnection> {c});

      var phoneCalls = groups
         .Select(group => new PhoneCall(group.ToList()))
         .ToList();

      var phoneCallsSorted = phoneCalls
         .OrderByDescending(x => x.CallDateTime)
         .ToList();

      return new PhoneCalls(phoneCallsSorted);
   }
}
