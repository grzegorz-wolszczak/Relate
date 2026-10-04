namespace Relate.AppLogic.CallProcessing;

public static class CallLogMatcher
{
   // Match call-log entries to a contact by phone number only (names can differ / change,
   // numbers are more stable). Value equality of RelatePhoneNumber does the matching.
   public static List<CallConnection> MatchCalls(
      IReadOnlyList<RelatePhoneNumber> contactPhoneNumbers,
      IEnumerable<CallLogEntry> callLog)
   {
      var connections = new List<CallConnection>();

      foreach (var call in callLog)
      {
         if (string.IsNullOrEmpty(call.Number))
         {
            continue;
         }

         var callNumber = new RelatePhoneNumber(call.Number);
         if (!contactPhoneNumbers.Contains(callNumber))
         {
            continue;
         }

         connections.Add(new CallConnection
         {
            CallDate = call.Date,
            CallDuration = TimeSpan.FromSeconds(call.DurationSeconds),
         });
      }

      return connections;
   }
}
