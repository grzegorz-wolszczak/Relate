namespace Relate.AppLogic.CallProcessing;

public record PhoneCall
{
   // if there were more than one connection for that day
   // assume CallDate as first call of that date (oldest call that day)
   // todo: fix setting call Date
   public PhoneCall(List<CallConnection> callConnections)
   {
      if (!callConnections.Any())
      {
         throw new InvalidOperationException("call connections are empty");
      }

      if (!AllOnSameDay(callConnections))
      {
         throw new InvalidOperationException("not all calls are on the same day");
      }

      var sortedFromNewest = callConnections
         .OrderByDescending(c => c.CallDate)
         .ToList();

      CallDateTime = sortedFromNewest.Last().CallDate;
      CallDuration = TimeSpan.FromTicks(callConnections.Sum(x => x.CallDuration.Ticks));
      CallConnections = sortedFromNewest;
   }


   public DateTimeOffset CallDateTime { get; }
   public TimeSpan CallDuration { get; }
   public List<CallConnection> CallConnections { get; }
   public bool IsAggregated => CallConnections.Count > 1;


   private static bool AllOnSameDay(IEnumerable<CallConnection> calls)
   {
      return calls
         .Select(c => c.CallDate.Date)
         .Distinct()
         .Count() <= 1;
   }
}