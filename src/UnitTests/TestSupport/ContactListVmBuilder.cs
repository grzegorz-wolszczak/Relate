using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.Services;
using Relate.Storage;
using Relate.ViewModels;


namespace UnitTests.TestSupport;

/// <summary>Single construction point for <see cref="ContactListVm"/> in tests.</summary>
public sealed class ContactListVmBuilder
{
   public FakePreferences Preferences { get; } = new();
   public FakeFileStore FileStore { get; } = new();
   public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero));
   public ProblemReporterVm Reporter { get; } =
      new(NullLogger<ProblemReporterVm>.Instance, new ImmediateDispatcher());

   public Mock<IContactDataEnricher> Enricher { get; } = new();
   public Mock<IContactService> ContactService { get; } = new();
   public Mock<INavigationService> Navigation { get; } = new();
   public Mock<IDialogService> Dialogs { get; } = new();
   public Mock<IBackupPathProvider> BackupPath { get; } = new();
   public Mock<IStoragePermission> StoragePermission { get; } = new();
   public Mock<ICallService> CallService { get; } = new();

   public ContactListVmBuilder()
   {
      Enricher.Setup(e => e.EnrichContactsWithSystemData(It.IsAny<IEnumerable<RelateContactVm>>()))
         .Returns(Task.CompletedTask);
      Enricher.Setup(e => e.EnrichContactWithSystemData(It.IsAny<RelateContactVm>()))
         .Returns(Task.CompletedTask);
      StoragePermission.Setup(p => p.HasAllFilesAccess()).Returns(true);
   }

   public ContactListVm Build() => new(
      new StorageService(Preferences, FileStore),
      Enricher.Object,
      Reporter,
      new CallScheduleCalculator(),
      ContactService.Object,
      Clock,
      Navigation.Object,
      Dialogs.Object,
      BackupPath.Object,
      StoragePermission.Object,
      CallService.Object,
      new ImmediateDispatcher());
}
