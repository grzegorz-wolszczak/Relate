using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.Services;
using Relate.ViewModels;
using static UnitTests.TestSupport.Build;

namespace UnitTests.ViewModels;

public class RelateContactVmTests
{
   private static readonly DateTimeOffset Now = At.Utc(2026, 8, 27, 12);

   [Fact]
   public void Constructor_rejects_a_null_contact_remover()
   {
      // Act
      var act = () => new RelateContactVm(
         null!, null!, new CallScheduleCalculator(), TimeProvider.System,
         Mock.Of<INavigationService>(), Mock.Of<IDialogService>(), Mock.Of<ICallService>());

      // Assert
      act.Should().Throw<ArgumentNullException>();
   }

   [Fact]
   public void Setting_call_connections_recalculates_the_next_call_deterministically()
   {
      // Arrange
      var builder = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(noContactPeriodDays: 30, longCallMinutes: 30, aggregate: true)
         .WithConnections(Call(At.Utc(2026, 8, 20), 45));

      // Act
      var vm = builder.Build();

      // Assert
      vm.NextCallInDays.Should().Be(23);
   }

   [Fact]
   public void Changing_the_no_call_period_recalculates()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(30, 30, aggregate: true)
         .WithConnections(Call(At.Utc(2026, 8, 20), 45))
         .Build();

      // Act
      vm.NoContactPeriodDays = 10;

      // Assert
      // last long call 2026-08-20 + 10 days = 2026-08-30; now 2026-08-27 -> 3 days
      vm.NextCallInDays.Should().Be(3);
   }

   [Fact]
   public void Raising_the_long_call_threshold_can_drop_the_last_long_call()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(30, longCallMinutes: 30, aggregate: true)
         .WithConnections(Call(At.Utc(2026, 8, 20), 45))
         .Build();
      vm.NextCallInDays.Should().Be(23);

      // Act
      vm.LongCallDurationMinutes = 60; // 45-min call no longer counts

      // Assert
      vm.NextCallInDays.Should().Be(0);
   }

   [Fact]
   public void Toggling_aggregation_re_evaluates_whether_same_day_calls_are_a_long_call()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(30, longCallMinutes: 30, aggregate: true)
         .WithConnections(Call(At.Utc(2026, 8, 20, 9), 20), Call(At.Utc(2026, 8, 20, 20), 20))
         .Build();
      vm.NextCallInDays.Should().Be(23); // 40 min combined -> long call

      // Act
      vm.ShouldAggregateConnectionByDay = false;

      // Assert
      vm.NextCallInDays.Should().Be(0); // 20 min each -> no long call
   }

   [Fact]
   public void Setting_a_manual_contact_date_recalculates_next_call()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Now(Now).WithSettings(30, 30, aggregate: true).Build();
      vm.NextCallInDays.Should().Be(0); // no calls, no manual dates yet

      // Act
      var meeting = vm.ManualContactEntries.Single(e => e.Type == ContactType.Meeting);
      meeting.Date = Now.AddDays(-10);

      // Assert
      vm.NextCallInDays.Should().Be(20);
      vm.LastContactType.Should().Be(ContactType.Meeting);
      vm.LastContactCardDisplay.Should().Contain("meeting");
   }

   [Fact]
   public void Clearing_a_manual_contact_date_recalculates_next_call()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(30, 30, aggregate: true)
         .WithManualContactDate(ContactType.Meeting, Now.AddDays(-10))
         .Build();
      vm.NextCallInDays.Should().Be(20);

      // Act
      vm.ManualContactEntries.Single(e => e.Type == ContactType.Meeting).ClearCommand.Execute(null);

      // Assert
      vm.NextCallInDays.Should().Be(0);
      vm.LastContactDateTime.Should().BeNull();
   }

   [Fact]
   public void Contact_image_color_is_deterministic_and_tracks_name_and_id()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Named("Ada").WithId("id-1").Build();
      var sameColor = vm.ContactImageColor;

      // Act
      vm.ContactDisplayName = "Grace";

      // Assert
      vm.ContactImageColor.Should().Be(vm.ContactImageColor); // stable within a run
      vm.ContactImageColor.Should().NotBe(sameColor).And.NotBeNull();
   }

   [Fact]
   public void Display_strings_reflect_the_current_state()
   {
      // Arrange
      var vm = new RelateContactVmBuilder()
         .Now(Now)
         .WithSettings(30, 30, aggregate: true)
         .WithConnections(Call(At.Utc(2026, 8, 20), 45))
         .Build();

      // Act
      var nextCall = vm.NextCallInDaysDisplay;
      var lastCall = vm.LastContactCardDisplay;

      // Assert
      nextCall.Should().Be("next contact: in 23d");
      lastCall.Should().Contain("Contact: 7d ago <phone call>");
      lastCall.Should().Contain("2026-08-20");
      lastCall.Should().Contain("(took: 45min)");
   }

   [Fact]
   public void No_qualifying_calls_reads_as_today_and_na()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Now(Now).WithConnections().Build();

      // Act
      var nextCall = vm.NextCallInDaysDisplay;
      var lastCall = vm.LastContactCardDisplay;

      // Assert
      nextCall.Should().Be("next contact: today!");
      lastCall.Should().Be("Contact: never");
   }

   [Fact]
   public async Task Remove_command_with_no_id_logs_and_does_nothing_else()
   {
      // Arrange
      var remover = new Mock<IContactRemover>();
      var navigation = new Mock<INavigationService>();
      var vm = new RelateContactVmBuilder()
         .WithId(null).WithRemover(remover.Object).WithNavigation(navigation.Object)
         .Build();

      // Act
      await vm.RemoveContactCommand.ExecuteAsync(null);

      // Assert
      remover.Verify(r => r.RemoveContact(It.IsAny<string>()), Times.Never);
      navigation.Verify(n => n.GoBackAsync(), Times.Never);
   }

   [Fact]
   public async Task Remove_command_when_confirmed_removes_and_navigates_back()
   {
      // Arrange
      var remover = new Mock<IContactRemover>();
      remover.Setup(r => r.RemoveContact("c9")).Returns(true);
      var navigation = new Mock<INavigationService>();
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      var vm = new RelateContactVmBuilder()
         .WithId("c9").WithRemover(remover.Object).WithNavigation(navigation.Object).WithDialogs(dialogs.Object)
         .Build();

      // Act
      await vm.RemoveContactCommand.ExecuteAsync(null);

      // Assert
      remover.Verify(r => r.RemoveContact("c9"), Times.Once);
      navigation.Verify(n => n.GoBackAsync(), Times.Once);
   }

   [Fact]
   public async Task Remove_command_when_declined_does_not_remove()
   {
      // Arrange
      var remover = new Mock<IContactRemover>();
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(false);
      var vm = new RelateContactVmBuilder()
         .WithId("c9").WithRemover(remover.Object).WithDialogs(dialogs.Object)
         .Build();

      // Act
      await vm.RemoveContactCommand.ExecuteAsync(null);

      // Assert
      remover.Verify(r => r.RemoveContact(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task Remove_command_does_not_navigate_when_removal_fails()
   {
      // Arrange
      var remover = new Mock<IContactRemover>();
      remover.Setup(r => r.RemoveContact(It.IsAny<string>())).Returns(false);
      var navigation = new Mock<INavigationService>();
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      var vm = new RelateContactVmBuilder()
         .WithId("c9").WithRemover(remover.Object).WithNavigation(navigation.Object).WithDialogs(dialogs.Object)
         .Build();

      // Act
      await vm.RemoveContactCommand.ExecuteAsync(null);

      // Assert
      navigation.Verify(n => n.GoBackAsync(), Times.Never);
   }

   [Fact]
   public async Task Back_command_navigates_back()
   {
      // Arrange
      var navigation = new Mock<INavigationService>();
      var vm = new RelateContactVmBuilder().WithNavigation(navigation.Object).Build();

      // Act
      await vm.HandleBackButtonCommand.ExecuteAsync(null);

      // Assert
      navigation.Verify(n => n.GoBackAsync(), Times.Once);
   }

   [Fact]
   public void HasPhoneNumbers_is_true_when_contact_has_numbers()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().WithNumbers("123456789").Build();

      // Assert
      vm.HasPhoneNumbers.Should().BeTrue();
   }

   [Fact]
   public void HasPhoneNumbers_is_false_when_contact_has_no_numbers()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().WithNoPhoneNumbers().Build();

      // Assert
      vm.HasPhoneNumbers.Should().BeFalse();
   }

   [Fact]
   public async Task CallContact_when_no_numbers_shows_alert_and_never_places_a_call()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      var callService = new Mock<ICallService>();
      var vm = new RelateContactVmBuilder()
         .WithNoPhoneNumbers().WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      dialogs.Verify(d => d.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
      callService.Verify(c => c.EnsureCallPermissionAsync(), Times.Never);
      callService.Verify(c => c.PlaceCall(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task CallContact_when_permission_declined_and_user_declines_settings_cancels_the_call()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(false);
      var callService = new Mock<ICallService>();
      callService.Setup(c => c.EnsureCallPermissionAsync()).ReturnsAsync(false);
      var vm = new RelateContactVmBuilder()
         .WithNumbers("123456789").WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      dialogs.Verify(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
      callService.Verify(c => c.OpenCallPermissionSettings(), Times.Never);
      callService.Verify(c => c.PlaceCall(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task CallContact_when_permission_declined_and_user_opens_settings_cancels_the_call_too()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      var callService = new Mock<ICallService>();
      callService.Setup(c => c.EnsureCallPermissionAsync()).ReturnsAsync(false);
      var vm = new RelateContactVmBuilder()
         .WithNumbers("123456789").WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      callService.Verify(c => c.OpenCallPermissionSettings(), Times.Once);
      dialogs.Verify(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
      callService.Verify(c => c.PlaceCall(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task CallContact_when_picker_cancelled_does_not_place_call()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
         .ReturnsAsync((string?)null);
      var callService = new Mock<ICallService>();
      callService.Setup(c => c.EnsureCallPermissionAsync()).ReturnsAsync(true);
      var vm = new RelateContactVmBuilder()
         .WithNumbers("123456789").WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      callService.Verify(c => c.PlaceCall(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task CallContact_with_single_number_still_shows_picker()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
         .ReturnsAsync((string?)null);
      var callService = new Mock<ICallService>();
      callService.Setup(c => c.EnsureCallPermissionAsync()).ReturnsAsync(true);
      var vm = new RelateContactVmBuilder()
         .WithNumbers("123456789").WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      dialogs.Verify(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
   }

   [Fact]
   public async Task CallContact_places_call_to_the_selected_number()
   {
      // Arrange
      var dialogs = new Mock<IDialogService>();
      dialogs.Setup(d => d.PickOptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
         .Returns<string, string, string[]>((_, _, options) => Task.FromResult<string?>(options[1]));
      var callService = new Mock<ICallService>();
      callService.Setup(c => c.EnsureCallPermissionAsync()).ReturnsAsync(true);
      var vm = new RelateContactVmBuilder()
         .WithNumbers("111", "222").WithDialogs(dialogs.Object).WithCallService(callService.Object)
         .Build();

      // Act
      await vm.CallContactCommand.ExecuteAsync(null);

      // Assert
      callService.Verify(c => c.PlaceCall("222"), Times.Once);
   }
}