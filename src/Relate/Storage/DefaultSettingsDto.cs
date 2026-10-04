using System.Text.Json.Serialization;

namespace Relate.Storage;

public sealed class DefaultSettingsDto
{
    private static readonly TimeSpan DefaultMaxNoCallingDuration = TimeSpan.FromDays(30);
    private static readonly TimeSpan DefaultProperCallDuration = TimeSpan.FromMinutes(30);

    [JsonPropertyName("maxNoCallingDuration")]
    public TimeSpan NoContactPeriod { get; set; } = DefaultMaxNoCallingDuration;

    [JsonPropertyName("properCallDuration")]
    public TimeSpan ProperCallDuration { get; set; } = DefaultProperCallDuration;


    // JSON name kept as the historical "shouldCalculateConnectionByDay" on purpose -
    // renaming it would orphan the value in already-persisted settings.
    [JsonPropertyName("shouldCalculateConnectionByDay")]
    public bool ShouldAggregateConnectionByDay { get; set; } = false;

}