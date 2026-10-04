namespace Relate.AppLogic.CallProcessing;

public sealed record class CallConnection
{
   public required DateTimeOffset CallDate { get; init; }
   public required TimeSpan CallDuration { get; init; }
}