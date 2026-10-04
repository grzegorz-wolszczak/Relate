using System.Text.Json;
using Relate.Storage;

namespace UnitTests.Storage;

public class StorageServicePreferencesTests
{
   private const string ContactsKey = "SerializedListKey";
   private const string SettingsKey = "DefaultSettingsKey";

   private readonly FakePreferences _prefs = new();
   private readonly StorageService _sut;

   public StorageServicePreferencesTests() => _sut = new StorageService(_prefs, new FakeFileStore());

   private static RelateContactDto Contact(string id, string name = "n") => new()
   {
      Id = id,
      DisplayName = name,
      MinCallDuration = TimeSpan.FromMinutes(30),
      MaxNoContactDuration = TimeSpan.FromDays(30),
      PhoneNumbers = [],
   };

   [Fact]
   public void Reading_contacts_with_no_stored_value_returns_an_empty_list()
   {
      // Act
      var contacts = _sut.ReadContactsFromStorage();

      // Assert
      contacts.Should().BeEmpty();
   }

   [Fact]
   public void Contacts_round_trip_through_preferences()
   {
      // Arrange
      var contacts = new List<RelateContactDto> { Contact("1", "Ann"), Contact("2", "Bob") };

      // Act
      _sut.SaveContacts(contacts);
      var reloaded = _sut.ReadContactsFromStorage();

      // Assert
      reloaded.Should().BeEquivalentTo(contacts);
   }

   [Fact]
   public void Saving_contacts_drops_duplicate_ids_keeping_the_first()
   {
      // Arrange
      var contacts = new List<RelateContactDto> { Contact("1", "First"), Contact("1", "Second"), Contact("2", "Other") };

      // Act
      _sut.SaveContacts(contacts);
      var stored = _sut.ReadContactsFromStorage();

      // Assert
      stored.Should().HaveCount(2);
      stored.Single(c => c.Id == "1").DisplayName.Should().Be("First");
   }

   [Fact]
   public void An_empty_stored_string_is_treated_as_no_value()
   {
      // Arrange
      _prefs.Set(ContactsKey, "   ", null);

      // Act
      var contacts = _sut.ReadContactsFromStorage();

      // Assert
      contacts.Should().BeEmpty();
   }

   [Fact]
   public void Reading_default_settings_with_nothing_stored_returns_fresh_defaults()
   {
      // Act
      var settings = _sut.ReadDefaultSettings();

      // Assert
      settings.NoContactPeriod.Should().Be(TimeSpan.FromDays(30));
      settings.ProperCallDuration.Should().Be(TimeSpan.FromMinutes(30));
      settings.ShouldAggregateConnectionByDay.Should().BeFalse();
   }

   [Fact]
   public void Default_settings_round_trip_through_preferences()
   {
      // Arrange
      var settings = new DefaultSettingsDto
      {
         NoContactPeriod = TimeSpan.FromDays(45),
         ProperCallDuration = TimeSpan.FromMinutes(90),
         ShouldAggregateConnectionByDay = true,
      };

      // Act
      _sut.SaveDefaultSettings(settings);
      var reloaded = _sut.ReadDefaultSettings();

      // Assert
      reloaded.Should().BeEquivalentTo(settings);
   }

   [Fact]
   public void Malformed_stored_json_surfaces_as_a_JsonException()
   {
      // Arrange
      _prefs.Set(SettingsKey, "{ not json", null);

      // Act
      var act = () => _sut.ReadDefaultSettings();

      // Assert
      act.Should().Throw<JsonException>();
   }
}
