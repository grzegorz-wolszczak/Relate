using System.Text.Json.Serialization;
using Relate.AppLogic.CallProcessing;

namespace Relate.Storage;

public class RelateContactDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } // this is from call log, unique Id
    [JsonPropertyName("maxCallDuration")]
    public TimeSpan MinCallDuration { get; set; }

    // JSON name kept as the historical "maxNoCallsDuration" on purpose - renaming it would
    // orphan the value in already-persisted contacts/backups.
    [JsonPropertyName("maxNoCallsDuration")]
    public TimeSpan MaxNoContactDuration { get; set; }

    // Missing in older stored contacts -> deserializes to false (calls counted individually).
    [JsonPropertyName("shouldAggregateConnectionByDay")]
    public bool ShouldAggregateConnectionByDay { get; set; }

    // Missing in older stored contacts -> deserializes to an empty list (no manual dates set).
    [JsonPropertyName("manualContactDates")]
    public List<ManualContactDateDto> ManualContactDates { get; set; } = new();

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("phoneNumbers")]
    public List<string> PhoneNumbers { get; set; } = new();
}

public class ManualContactDateDto
{
    [JsonPropertyName("type")]
    public ContactType Type { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }
}