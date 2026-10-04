using System.Text.Json.Serialization;

namespace Relate.AppLogic.CallProcessing;

// New values must be appended at the end - the enum is persisted (see JsonStringEnumConverter
// below, which stores the name rather than the numeric value, but callers should not rely on it).
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContactType
{
   Call,
   Meeting,
   VideoCall
}

// Single place to register a manually-dated contact type (as opposed to Call, which is derived
// automatically from the call log). Adding a future type (e.g. a text message) means adding an
// enum value above and one entry here - no other file needs to change.
public static class ManualContactTypeCatalog
{
   public static readonly IReadOnlyList<(ContactType Type, string Label)> All = new[]
   {
      (ContactType.Meeting, "meeting"),
      (ContactType.VideoCall, "video call"),
   };

   public static string LabelFor(ContactType type) => type switch
   {
      ContactType.Call => "phone call",
      ContactType.Meeting => "meeting",
      ContactType.VideoCall => "video call",
      _ => "unknown"
   };
}
