using System.Text.Json;
using Relate.AppLogic.CallProcessing;
using Relate.Storage;
using Relate.ViewModels;

namespace UnitTests.CallProcessing;

public class ContactTypeTests
{
   private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

   public static TheoryData<ContactType> ManualTypes => new(ManualContactTypeCatalog.All.Select(c => c.Type));

   [Fact]
   public void Catalog_lists_in_person_video_call_and_other_contact_in_that_order()
   {
      // Assert
      ManualContactTypeCatalog.All.Should().Equal(
         (ContactType.InPerson, "in-person"),
         (ContactType.VideoCall, "video call"),
         (ContactType.Other, "other contact"));
   }

   [Fact]
   public void Every_catalog_label_matches_the_label_used_on_the_last_contact_card()
   {
      // Assert
      foreach (var (type, label) in ManualContactTypeCatalog.All)
      {
         ManualContactTypeCatalog.LabelFor(type).Should().Be(label);
      }
   }

   [Fact]
   public void A_view_model_has_one_manual_entry_per_catalog_type()
   {
      // Act
      var vm = new RelateContactVmBuilder().Build();

      // Assert
      vm.ManualContactEntries.Select(e => e.Type)
         .Should().Equal(ManualContactTypeCatalog.All.Select(c => c.Type));
   }

   [Theory]
   [MemberData(nameof(ManualTypes))]
   public void A_manual_contact_date_of_any_type_becomes_the_last_contact(ContactType type)
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Now(Now).WithSettings(30, 30, aggregate: true).Build();

      // Act
      vm.ManualContactEntries.Single(e => e.Type == type).Date = Now.AddDays(-10);

      // Assert
      vm.LastContactType.Should().Be(type);
      vm.NextCallInDays.Should().Be(20);
   }

   [Theory]
   [MemberData(nameof(ManualTypes))]
   public void The_most_recent_manual_type_wins_regardless_of_which_it_is(ContactType newest)
   {
      // Arrange
      var builder = new RelateContactVmBuilder().Now(Now).WithSettings(30, 30, aggregate: true);
      foreach (var (type, _) in ManualContactTypeCatalog.All)
      {
         builder.WithManualContactDate(type, type == newest ? Now.AddDays(-1) : Now.AddDays(-20));
      }

      // Act
      var vm = builder.Build();

      // Assert
      vm.LastContactType.Should().Be(newest);
      vm.NextCallInDays.Should().Be(29);
   }

   [Fact]
   public void Other_contact_more_recent_than_the_last_long_call_wins()
   {
      // Arrange
      var calls = PhoneCallProcessor.ProcessCallConnections(
         [Build.Call(Now.AddDays(-20), 45)], shouldAggregateByDay: true);
      var manualDates = new List<(ContactType Type, DateTimeOffset? Date)> {(ContactType.Other, Now.AddDays(-2))};

      // Act
      var result = new CallScheduleCalculator().RecalculateCallsRelatedData(calls, 30, 30, manualDates, Now);

      // Assert
      result.LastContactType.Should().Be(ContactType.Other);
      result.LastContactCallDuration.Should().BeNull();
      result.NextCallInDays.Should().Be(28);
   }

   [Theory]
   [MemberData(nameof(ManualTypes))]
   public void A_manual_date_of_any_type_survives_a_dto_round_trip(ContactType type)
   {
      // Arrange
      var date = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
      var vm = new RelateContactVmBuilder().WithId("1").Build();
      vm.SetManualContactDate(type, date);

      // Act
      var dto = new List<RelateContactVm> {vm}.ToContactsDto().Single();
      var json = JsonSerializer.Serialize(dto);
      var restored = JsonSerializer.Deserialize<RelateContactDto>(json)!;

      // Assert
      json.Should().Contain($"\"{type}\"");
      restored.ManualContactDates.Should().ContainSingle()
         .Which.Should().BeEquivalentTo(new ManualContactDateDto {Type = type, Date = date});
   }

   [Theory]
   [InlineData("Meeting", ContactType.InPerson)]
   [InlineData("meeting", ContactType.InPerson)]
   [InlineData("InPerson", ContactType.InPerson)]
   [InlineData("VideoCall", ContactType.VideoCall)]
   [InlineData("Other", ContactType.Other)]
   [InlineData("Call", ContactType.Call)]
   public void The_json_converter_reads_current_and_legacy_names(string name, ContactType expected)
   {
      // Act
      var type = JsonSerializer.Deserialize<ContactType>($"\"{name}\"");

      // Assert
      type.Should().Be(expected);
   }

   [Fact]
   public void The_json_converter_still_reads_numeric_values()
   {
      // Act
      var type = JsonSerializer.Deserialize<ContactType>("1");

      // Assert
      type.Should().Be(ContactType.InPerson);
   }

   [Fact]
   public void The_json_converter_rejects_an_unknown_name()
   {
      // Act
      var act = () => JsonSerializer.Deserialize<ContactType>("\"Carrier pigeon\"");

      // Assert
      act.Should().Throw<JsonException>();
   }

   [Fact]
   public void The_json_converter_never_writes_the_legacy_name()
   {
      // Act
      var json = JsonSerializer.Serialize(ContactType.InPerson);

      // Assert
      json.Should().Be("\"InPerson\"");
   }
}
