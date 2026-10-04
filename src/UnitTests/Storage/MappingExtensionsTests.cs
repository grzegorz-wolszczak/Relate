using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.Services;
using Relate.Storage;
using Relate.ViewModels;

namespace UnitTests.Storage;

public class MappingExtensionsTests
{
   private static readonly TimeProvider Clock = new FakeTimeProvider();

   private static ProblemReporterVm Reporter() =>
      new(NullLogger<ProblemReporterVm>.Instance, new ImmediateDispatcher());

   private static List<RelateContactVm> ToVms(List<RelateContactDto> dtos) =>
      dtos.ToRelateContacts(Mock.Of<IContactRemover>(), Reporter(), new CallScheduleCalculator(), Clock,
         Mock.Of<INavigationService>(), Mock.Of<IDialogService>(), Mock.Of<ICallService>());

   private static RelateContactDto Dto(string id, string name, int minutes, int days, bool aggregate) => new()
   {
      Id = id,
      DisplayName = name,
      MinCallDuration = TimeSpan.FromMinutes(minutes),
      MaxNoContactDuration = TimeSpan.FromDays(days),
      ShouldAggregateConnectionByDay = aggregate,
      PhoneNumbers = ["+48 123 456 789"],
   };

   [Fact]
   public void Dto_to_vm_maps_every_field()
   {
      // Arrange
      var dtos = new List<RelateContactDto> { Dto("42", "Ada", 30, 20, aggregate: true) };

      // Act
      var vm = ToVms(dtos).Single();

      // Assert
      vm.ContactId.Should().Be("42");
      vm.ContactDisplayName.Should().Be("Ada");
      vm.LongCallDurationMinutes.Should().Be(30);
      vm.NoContactPeriodDays.Should().Be(20);
      vm.ShouldAggregateConnectionByDay.Should().BeTrue();
      vm.PhoneNumbers.Single().Number.Should().Be("123456789");
   }

   [Fact]
   public void Dto_to_vm_keeps_durations_at_or_above_an_hour_intact()
   {
      // Arrange
      var dtos = new List<RelateContactDto> { Dto("1", "n", minutes: 90, days: 400, aggregate: false) };

      // Act
      var vm = ToVms(dtos).Single();

      // Assert
      vm.LongCallDurationMinutes.Should().Be(90);
      vm.NoContactPeriodDays.Should().Be(400);
   }

   [Fact]
   public void Vm_to_dto_maps_every_field()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .WithId("7").Named("Bob").WithNumbers("111", "222")
         .WithSettings(noContactPeriodDays: 15, longCallMinutes: 45, aggregate: true)
         .Build();

      // Act
      var dto = new List<RelateContactVm> { vm }.ToContactsDto().Single();

      // Assert
      dto.Id.Should().Be("7");
      dto.DisplayName.Should().Be("Bob");
      dto.MinCallDuration.Should().Be(TimeSpan.FromMinutes(45));
      dto.MaxNoContactDuration.Should().Be(TimeSpan.FromDays(15));
      dto.ShouldAggregateConnectionByDay.Should().BeTrue();
      dto.PhoneNumbers.Should().Equal("111", "222");
   }

   [Fact]
   public void Round_trip_dto_vm_dto_is_lossless_including_long_durations()
   {
      // Arrange
      var original = Dto("1", "n", minutes: 125, days: 45, aggregate: true);

      // Act
      var back = ToVms([original]).ToContactsDto().Single();

      // Assert
      back.MinCallDuration.Should().Be(TimeSpan.FromMinutes(125));
      back.MaxNoContactDuration.Should().Be(TimeSpan.FromDays(45));
   }

   [Fact]
   public void Empty_input_maps_to_empty_output()
   {
      // Act
      var vms = ToVms([]);
      var dtos = new List<RelateContactVm>().ToContactsDto();

      // Assert
      vms.Should().BeEmpty();
      dtos.Should().BeEmpty();
   }

   [Fact]
   public void Dto_to_vm_loads_manual_contact_dates()
   {
      // Arrange
      var meetingDate = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
      var dto = Dto("1", "Ada", 30, 20, aggregate: false);
      dto.ManualContactDates = [new ManualContactDateDto {Type = ContactType.Meeting, Date = meetingDate}];

      // Act
      var vm = ToVms([dto]).Single();

      // Assert
      vm.GetManualContactDate(ContactType.Meeting).Should().Be(meetingDate);
      vm.GetManualContactDate(ContactType.VideoCall).Should().BeNull();
   }

   [Fact]
   public void Vm_to_dto_round_trips_manual_contact_dates()
   {
      // Arrange
      var meetingDate = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
      var videoCallDate = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
      var vm = new RelateContactVmBuilder().WithId("1").Build();
      vm.SetManualContactDate(ContactType.Meeting, meetingDate);
      vm.SetManualContactDate(ContactType.VideoCall, videoCallDate);

      // Act
      var dto = new List<RelateContactVm> {vm}.ToContactsDto().Single();

      // Assert
      dto.ManualContactDates.Should().BeEquivalentTo(
      [
         new ManualContactDateDto {Type = ContactType.Meeting, Date = meetingDate},
         new ManualContactDateDto {Type = ContactType.VideoCall, Date = videoCallDate},
      ]);
   }
}
