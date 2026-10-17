using System.Text.Json;
using System.Text.Json.Serialization;

namespace Relate.AppLogic.CallProcessing;

// New values must be appended at the end - the enum is persisted (see ContactTypeJsonConverter
// below, which stores the name rather than the numeric value, but callers should not rely on it).
[JsonConverter(typeof(ContactTypeJsonConverter))]
public enum ContactType
{
   Call,
   InPerson, // was "Meeting" before the rename - still accepted when reading stored data
   VideoCall,
   Other
}

// Stores the enum name, like JsonStringEnumConverter, but also reads the legacy "Meeting" name
// that already-persisted contacts and backups contain.
public sealed class ContactTypeJsonConverter : JsonConverter<ContactType>
{
   private const string LegacyMeetingName = "Meeting";

   public override ContactType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if (reader.TokenType == JsonTokenType.Number)
      {
         return (ContactType)reader.GetInt32();
      }

      var name = reader.GetString();
      if (string.Equals(name, LegacyMeetingName, StringComparison.OrdinalIgnoreCase))
      {
         return ContactType.InPerson;
      }

      if (Enum.TryParse<ContactType>(name, ignoreCase: true, out var value))
      {
         return value;
      }

      throw new JsonException($"Unknown contact type '{name}'.");
   }

   public override void Write(Utf8JsonWriter writer, ContactType value, JsonSerializerOptions options)
   {
      writer.WriteStringValue(value.ToString());
   }
}

// Single place to register a manually-dated contact type (as opposed to Call, which is derived
// automatically from the call log). Adding a future type (e.g. a text message) means adding an
// enum value above and one entry here - no other file needs to change.
public static class ManualContactTypeCatalog
{
   public static readonly IReadOnlyList<(ContactType Type, string Label)> All = new[]
   {
      (ContactType.InPerson, "in-person"),
      (ContactType.VideoCall, "video call"),
      (ContactType.Other, "other contact"),
   };

   public static string LabelFor(ContactType type) => type switch
   {
      ContactType.Call => "phone call",
      ContactType.InPerson => "in-person",
      ContactType.VideoCall => "video call",
      ContactType.Other => "other contact",
      _ => "unknown"
   };
}
