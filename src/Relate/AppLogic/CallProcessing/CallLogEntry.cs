namespace Relate.AppLogic.CallProcessing;

public class CallLogEntry
{
    public required string? Number { get; set; }
    public required DateTimeOffset Date { get; set; }
    public required long DurationSeconds { get; set; }
    public required string? PersonWhoCalledName { get; set; }

}