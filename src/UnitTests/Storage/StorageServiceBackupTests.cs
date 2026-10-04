using System.Text.Json;
using Relate.Storage;
using Relate.ViewModels;

namespace UnitTests.Storage;

public class StorageServiceBackupTests
{
   private readonly FakeFileStore _files = new();
   private readonly StorageService _sut;

   public StorageServiceBackupTests() => _sut = new StorageService(new FakePreferences(), _files);

   private static ContactsList ListWith(params RelateContactVm[] contacts)
   {
      var list = new ContactsList([], new ImmediateDispatcher());
      list.Reset(contacts.ToList());
      return list;
   }

   [Fact]
   public async Task SaveBackup_writes_a_deduplicated_json_document()
   {
      // Arrange
      var list = ListWith(
         new RelateContactVmBuilder().WithId("1").Named("Ann").Build(),
         new RelateContactVmBuilder().WithId("1").Named("Ann duplicate").Build(),
         new RelateContactVmBuilder().WithId("2").Named("Bob").Build());

      // Act
      var result = await _sut.SaveBackup(list, "backup.json");

      // Assert
      result.HasValue.Should().BeFalse();
      var written = JsonSerializer.Deserialize<List<RelateContactDto>>(_files.Read("backup.json")!)!;
      written.Select(c => c.Id).Should().Equal("1", "2");
   }

   [Fact]
   public async Task SaveBackup_reports_a_write_failure_as_an_action_error()
   {
      // Arrange
      _files.FailWrites = true;

      // Act
      var result = await _sut.SaveBackup(ListWith(), "backup.json");

      // Assert
      result.HasValue.Should().BeTrue();
      result.Value.Message.Should().Contain("Could not save backup");
   }

   [Fact]
   public void IsBackupAvailable_reflects_the_file_store()
   {
      // Arrange
      var before = _sut.IsBackupAvailable("x.json");
      _files.Seed("x.json", "[]");

      // Act
      var after = _sut.IsBackupAvailable("x.json");

      // Assert
      before.Should().BeFalse();
      after.Should().BeTrue();
   }

   [Fact]
   public async Task RestoreFromBackup_returns_an_error_when_the_file_is_missing()
   {
      // Act
      var result = await _sut.RestoreFromBackup("missing.json");

      // Assert
      result.IsT1.Should().BeTrue();
      result.AsT1.Message.Should().Be("Backup file does not exist");
   }

   [Fact]
   public async Task RestoreFromBackup_returns_the_deserialized_contacts()
   {
      // Arrange
      _files.Seed("backup.json", """
         [ { "id": "a", "maxCallDuration": "00:30:00", "maxNoCallsDuration": "30.00:00:00", "displayName": "Ann", "phoneNumbers": [] } ]
         """);

      // Act
      var result = await _sut.RestoreFromBackup("backup.json");

      // Assert
      result.IsT0.Should().BeTrue();
      result.AsT0.Single().DisplayName.Should().Be("Ann");
   }

   [Fact]
   public async Task RestoreFromBackup_reports_malformed_json_as_an_action_error()
   {
      // Arrange
      _files.Seed("backup.json", "{ not json");

      // Act
      var result = await _sut.RestoreFromBackup("backup.json");

      // Assert
      result.IsT1.Should().BeTrue();
      result.AsT1.Message.Should().Contain("Could not restore backup");
   }
}
