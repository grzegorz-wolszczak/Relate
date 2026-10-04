using System.Text.Json;
using Relate.AppLogic.CallProcessing;
using Relate.Storage;

namespace UnitTests.Storage;

public class SettingsDtoJsonTests
{
   [Fact]
   public void DefaultSettingsDto_has_the_documented_defaults()
   {
      // Act
      var dto = new DefaultSettingsDto();

      // Assert
      dto.NoContactPeriod.Should().Be(TimeSpan.FromDays(30));
      dto.ProperCallDuration.Should().Be(TimeSpan.FromMinutes(30));
      dto.ShouldAggregateConnectionByDay.Should().BeFalse();
   }

   [Fact]
   public void DefaultSettingsDto_serializes_with_the_historical_json_names()
   {
      // Arrange
      var dto = new DefaultSettingsDto { ShouldAggregateConnectionByDay = true };

      // Act
      var json = JsonSerializer.Serialize(dto);

      // Assert
      json.Should().Contain("maxNoCallingDuration")
         .And.Contain("properCallDuration")
         .And.Contain("shouldCalculateConnectionByDay");
   }

   [Fact]
   public void DefaultSettingsDto_deserializes_the_historical_json_names()
   {
      // Arrange
      const string json = """
         { "maxNoCallingDuration": "14.00:00:00", "properCallDuration": "00:45:00", "shouldCalculateConnectionByDay": true }
         """;

      // Act
      var dto = JsonSerializer.Deserialize<DefaultSettingsDto>(json)!;

      // Assert
      dto.NoContactPeriod.Should().Be(TimeSpan.FromDays(14));
      dto.ProperCallDuration.Should().Be(TimeSpan.FromMinutes(45));
      dto.ShouldAggregateConnectionByDay.Should().BeTrue();
   }

   [Fact]
   public void DefaultSettingsDto_missing_fields_fall_back_to_defaults()
   {
      // Act
      var dto = JsonSerializer.Deserialize<DefaultSettingsDto>("{}")!;

      // Assert
      dto.NoContactPeriod.Should().Be(TimeSpan.FromDays(30));
      dto.ShouldAggregateConnectionByDay.Should().BeFalse();
   }

   [Fact]
   public void RelateContactDto_uses_its_json_names_including_the_maxCallDuration_alias()
   {
      // Arrange
      var dto = new RelateContactDto
      {
         Id = "1",
         DisplayName = "Ann",
         MinCallDuration = TimeSpan.FromMinutes(30),
         MaxNoContactDuration = TimeSpan.FromDays(20),
         ShouldAggregateConnectionByDay = true,
         PhoneNumbers = ["123"],
      };

      // Act
      var json = JsonSerializer.Serialize(dto);
      var round = JsonSerializer.Deserialize<RelateContactDto>(json)!;

      // Assert
      json.Should().Contain("\"maxCallDuration\"").And.Contain("\"maxNoCallsDuration\"").And.Contain("\"displayName\"");
      round.Should().BeEquivalentTo(dto);
   }

   [Fact]
   public void RelateContactDto_legacy_json_without_aggregate_flag_defaults_to_false()
   {
      // Arrange
      const string json = """
         { "id": "x", "maxCallDuration": "00:30:00", "maxNoCallsDuration": "30.00:00:00", "displayName": "Old", "phoneNumbers": [] }
         """;

      // Act
      var dto = JsonSerializer.Deserialize<RelateContactDto>(json)!;

      // Assert
      dto.ShouldAggregateConnectionByDay.Should().BeFalse();
   }

   [Fact]
   public void RelateContactDto_legacy_json_without_manual_contact_dates_defaults_to_empty_list()
   {
      // Arrange
      const string json = """
         { "id": "x", "maxCallDuration": "00:30:00", "maxNoCallsDuration": "30.00:00:00", "displayName": "Old", "phoneNumbers": [] }
         """;

      // Act
      var dto = JsonSerializer.Deserialize<RelateContactDto>(json)!;

      // Assert
      dto.ManualContactDates.Should().NotBeNull().And.BeEmpty();
   }

   [Fact]
   public void RelateContactDto_round_trips_manual_contact_dates_with_readable_type_names()
   {
      // Arrange
      var dto = new RelateContactDto
      {
         Id = "1",
         DisplayName = "Ann",
         PhoneNumbers = [],
         ManualContactDates =
         [
            new ManualContactDateDto { Type = ContactType.Meeting, Date = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero) }
         ]
      };

      // Act
      var json = JsonSerializer.Serialize(dto);
      var round = JsonSerializer.Deserialize<RelateContactDto>(json)!;

      // Assert
      json.Should().Contain("\"manualContactDates\"").And.Contain("\"Meeting\"");
      round.ManualContactDates.Should().BeEquivalentTo(dto.ManualContactDates);
   }

   [Fact]
   public void RelateContactDto_phone_numbers_default_to_an_empty_list()
   {
      // Act
      var dto = new RelateContactDto();

      // Assert
      dto.PhoneNumbers.Should().NotBeNull().And.BeEmpty();
   }
}
