using OneOf;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.AppLogic.Utils.OneOfActions;
using Relate.Storage;
using Relate.ViewModels;
using static UnitTests.TestSupport.Build;

namespace UnitTests.ViewModels;

public class ContactListVmTests
{
   private readonly ContactListVmBuilder _b = new();

   private static OneOf<string, ActionError, ActionMissingPermission> Path(string p) => p;
   private static OneOf<string, ActionError, ActionMissingPermission> PathError(string m) => new ActionError(m);
   private static OneOf<string, ActionError, ActionMissingPermission> PathMissingPermission(string m) => new ActionMissingPermission(m);

   [Fact]
   public async Task LoadAsync_reads_the_default_settings_into_the_view_model()
   {
      // Arrange
      _b.Preferences.Set("DefaultSettingsKey", """
         { "maxNoCallingDuration": "14.00:00:00", "properCallDuration": "01:30:00", "shouldCalculateConnectionByDay": true }
         """, null);
      var vm = _b.Build();

      // Act
      await vm.LoadAsync();

      // Assert
      vm.NoContactPeriodDays.Should().Be(14);
      vm.ProperCallDurationMinutes.Should().Be(90);
      vm.ShouldAggregateConnectionByDay.Should().BeTrue();
   }

   [Fact]
   public void Save_persists_contacts_and_default_settings()
   {
      // Arrange
      var vm = _b.Build();
      vm.NoContactPeriodDays = 21;
      vm.ProperCallDurationMinutes = 40;
      vm.ShouldAggregateConnectionByDay = true;

      // Act
      vm.Save();

      // Assert
      var storage = new StorageService(_b.Preferences, _b.FileStore);
      var settings = storage.ReadDefaultSettings();
      settings.NoContactPeriod.Should().Be(TimeSpan.FromDays(21));
      settings.ProperCallDuration.Should().Be(TimeSpan.FromMinutes(40));
      _b.Preferences.ContainsKey("SerializedListKey", null).Should().BeTrue();
   }

   [Fact]
   public async Task Selecting_a_contact_navigates_to_the_details_route_and_clears_the_selection()
   {
      // Arrange
      var vm = _b.Build();
      var contact = new RelateContactVmBuilder().WithId("c1").Build();
      vm.SelectedContact = contact;

      // Act
      await vm.SelectionChangedCommand.ExecuteAsync(null);

      // Assert
      _b.Navigation.Verify(n => n.GoToAsync("//ContactListPage/ContactDetailsPage"), Times.Once);
      vm.SelectedRelateContact.Should().BeSameAs(contact);
      vm.SelectedContact.Should().BeNull();
   }

   [Fact]
   public async Task Selecting_a_contact_without_an_id_does_not_navigate()
   {
      // Arrange
      var vm = _b.Build();
      vm.SelectedContact = new RelateContactVmBuilder().WithId(null).Build();

      // Act
      await vm.SelectionChangedCommand.ExecuteAsync(null);

      // Assert
      _b.Navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
   }

   [Fact]
   public async Task Backup_writes_a_file_when_confirmed()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(Path("backup.json"));
      _b.Dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.FileStore.Exists("backup.json").Should().BeTrue();
   }

   [Fact]
   public async Task Backup_is_skipped_when_the_user_declines()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(Path("backup.json"));
      _b.Dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(false);
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.FileStore.Exists("backup.json").Should().BeFalse();
      _b.Reporter.LogContent.Should().Contain("cancelled");
   }

   [Fact]
   public async Task Backup_path_error_is_logged_and_shown_in_a_dialog()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(PathError("no storage permission"));
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.Reporter.LogContent.Should().Contain("no storage permission");
      _b.Dialogs.Verify(d => d.AlertAsync(
         "Backup error",
         It.Is<string>(s => s.Contains("no storage permission")),
         "OK"), Times.Once);
   }

   [Fact]
   public async Task Restore_path_error_is_shown_in_a_dialog()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(PathError("no storage permission"));
      var vm = _b.Build();

      // Act
      await vm.RestoreContactsFromBackupCommand.ExecuteAsync(null);

      // Assert
      _b.Dialogs.Verify(d => d.AlertAsync(
         "Backup error",
         It.Is<string>(s => s.Contains("no storage permission")),
         "OK"), Times.Once);
   }

   [Fact]
   public async Task Backup_write_failure_is_shown_in_a_dialog()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(Path("backup.json"));
      _b.Dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      _b.FileStore.FailWrites = true;
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.Dialogs.Verify(d => d.AlertAsync(
         "Backup error",
         It.Is<string>(s => s.Contains("Could not save backup")),
         "OK"), Times.Once);
   }

   [Fact]
   public async Task Backup_missing_permission_opens_the_system_settings_when_the_user_agrees()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync())
         .ReturnsAsync(PathMissingPermission("All files access"));
      _b.Dialogs.Setup(d => d.ConfirmAsync("Permission required", It.IsAny<string>(), "Open settings", "Cancel"))
         .ReturnsAsync(true);
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.StoragePermission.Verify(p => p.OpenAllFilesAccessSettings(), Times.Once);
      _b.FileStore.Exists("backup.json").Should().BeFalse();
   }

   [Fact]
   public async Task Backup_missing_permission_does_nothing_when_the_user_cancels()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync())
         .ReturnsAsync(PathMissingPermission("All files access"));
      _b.Dialogs.Setup(d => d.ConfirmAsync("Permission required", It.IsAny<string>(), "Open settings", "Cancel"))
         .ReturnsAsync(false);
      var vm = _b.Build();

      // Act
      await vm.CreateContactsBackupCommand.ExecuteAsync(null);

      // Assert
      _b.StoragePermission.Verify(p => p.OpenAllFilesAccessSettings(), Times.Never);
   }

   [Fact]
   public async Task Restore_missing_permission_prompts_to_open_the_system_settings()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync())
         .ReturnsAsync(PathMissingPermission("All files access"));
      _b.Dialogs.Setup(d => d.ConfirmAsync("Permission required", It.IsAny<string>(), "Open settings", "Cancel"))
         .ReturnsAsync(true);
      var vm = _b.Build();

      // Act
      await vm.RestoreContactsFromBackupCommand.ExecuteAsync(null);

      // Assert
      _b.StoragePermission.Verify(p => p.OpenAllFilesAccessSettings(), Times.Once);
   }

   [Fact]
   public async Task Restore_warns_when_no_backup_file_exists()
   {
      // Arrange
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(Path("missing.json"));
      var vm = _b.Build();

      // Act
      await vm.RestoreContactsFromBackupCommand.ExecuteAsync(null);

      // Assert
      _b.Dialogs.Verify(d => d.AlertAsync("Warning", "No backup available", "Ok"), Times.Once);
   }

   [Fact]
   public async Task Restore_loads_contacts_from_the_backup_file_when_confirmed()
   {
      // Arrange
      _b.FileStore.Seed("backup.json", """
         [ { "id": "r1", "maxCallDuration": "00:30:00", "maxNoCallsDuration": "30.00:00:00", "displayName": "Restored", "phoneNumbers": ["111"] } ]
         """);
      _b.BackupPath.Setup(p => p.GetContactBackupFilePathAsync()).ReturnsAsync(Path("backup.json"));
      _b.Dialogs.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
         .ReturnsAsync(true);
      var vm = _b.Build();

      // Act
      await vm.RestoreContactsFromBackupCommand.ExecuteAsync(null);

      // Assert
      vm.ObservableContacts.Should().ContainSingle().Which.ContactDisplayName.Should().Be("Restored");
   }

   [Fact]
   public void RemoveContact_delegates_to_the_inner_list()
   {
      // Arrange
      var vm = _b.Build();

      // Act
      var removed = vm.RemoveContact("does-not-exist");

      // Assert
      removed.Should().BeFalse();
   }

   [Fact]
   public async Task Refresh_reloads_contacts_and_clears_the_refreshing_flag()
   {
      // Arrange
      var vm = _b.Build();

      // Act
      await vm.RefreshContactListCommand.ExecuteAsync(null);

      // Assert
      vm.IsListRefreshing.Should().BeFalse();
      _b.Enricher.Verify(e => e.EnrichContactsWithSystemData(It.IsAny<IEnumerable<RelateContactVm>>()), Times.AtLeastOnce);
   }

   [Fact]
   public void Stopping_typing_filters_the_list_and_keeps_the_search_bar_focused()
   {
      // Arrange
      var vm = _b.Build();
      vm.SearchBarText = "ann";

      // Act
      vm.UserStoppedTypingInSearchBarCommand.Execute("ann");

      // Assert
      vm.IsSearchBarFocused.Should().BeTrue();
   }

   [Fact]
   public async Task Adding_a_contact_that_is_already_in_the_list_shows_a_warning()
   {
      // Arrange
      var existing = SystemContact("dup", "Existing", "111");
      _b.ContactService.Setup(c => c.PickContactAsync())
         .ReturnsAsync((OneOf<MySystemContact, ActionCancelled, ActionError>)existing);
      var vm = _b.Build();
      _b.Preferences.Set("SerializedListKey", """
         [ { "id": "dup", "maxCallDuration": "00:30:00", "maxNoCallsDuration": "30.00:00:00", "displayName": "Existing", "phoneNumbers": [] } ]
         """, null);
      await vm.LoadAsync();

      // Act
      await vm.AddContactCommand.ExecuteAsync(null);

      // Assert
      _b.Dialogs.Verify(d => d.AlertAsync("Warning", "Contact is already in the list", "Ok"), Times.Once);
   }

   [Fact]
   public async Task Adding_a_contact_logs_an_error_when_picking_fails()
   {
      // Arrange
      _b.ContactService.Setup(c => c.PickContactAsync())
         .ReturnsAsync((OneOf<MySystemContact, ActionCancelled, ActionError>)new ActionError("picker exploded"));
      var vm = _b.Build();

      // Act
      await vm.AddContactCommand.ExecuteAsync(null);

      // Assert
      _b.Reporter.LogContent.Should().Contain("picker exploded");
   }

   [Fact]
   public async Task Cancelling_the_contact_picker_is_a_no_op()
   {
      // Arrange
      _b.ContactService.Setup(c => c.PickContactAsync())
         .ReturnsAsync((OneOf<MySystemContact, ActionCancelled, ActionError>)ActionCancelled.Instance);
      var vm = _b.Build();

      // Act
      var act = () => vm.AddContactCommand.ExecuteAsync(null);

      // Assert
      await act.Should().NotThrowAsync();
      vm.ObservableContacts.Should().BeEmpty();
   }

   [Fact]
   public async Task Picking_a_new_contact_adds_it_to_the_list_with_the_current_default_settings()
   {
      // Arrange
      _b.ContactService.Setup(c => c.PickContactAsync())
         .ReturnsAsync((OneOf<MySystemContact, ActionCancelled, ActionError>)SystemContact("new-1", "Fresh Face", "555"));
      var vm = _b.Build();
      vm.NoContactPeriodDays = 21;
      vm.ProperCallDurationMinutes = 40;
      vm.ShouldAggregateConnectionByDay = true;

      // Act
      await vm.AddContactCommand.ExecuteAsync(null);

      // Assert
      var added = vm.ObservableContacts.Should().ContainSingle().Which;
      added.ContactId.Should().Be("new-1");
      added.ContactDisplayName.Should().Be("Fresh Face");
      added.NoContactPeriodDays.Should().Be(21);
      added.ShouldAggregateConnectionByDay.Should().BeTrue();
   }
}
