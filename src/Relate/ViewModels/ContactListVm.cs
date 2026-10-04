using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.Pages;
using Relate.Services;
using Relate.Storage;

namespace Relate.ViewModels;

public partial class ContactListVm : BaseVm, IContactRemover
{
   private readonly StorageService _storageService;
   private readonly IContactDataEnricher _dataEnricher;
   private readonly ProblemReporterVm _problemReporter;
   private readonly CallScheduleCalculator _callScheduleCalculator;
   private readonly IContactService _contactService;
   private readonly TimeProvider _timeProvider;
   private readonly INavigationService _navigation;
   private readonly IDialogService _dialogs;
   private readonly IBackupPathProvider _backupPathProvider;
   private readonly IStoragePermission _storagePermission;
   private readonly ICallService _callService;

   //public ObservableCollection<RelateContact> LegacyContactList { get; private set; }
   public ObservableCollection<RelateContactVm> ObservableContacts { get; private set; }

   private readonly ContactsList _contactList;

   [ObservableProperty]
   private object? _selectedContact;

   // using double instead of int because NumericPicker native value is double
   [ObservableProperty]
   private int _noContactPeriodDays;

   // using double instead of int because NumericPicker native value is double
   [ObservableProperty]
   private int _properCallDurationMinutes;

   [ObservableProperty]
   private bool _isListRefreshing;

   [ObservableProperty]
   private string _searchBarText;

   [ObservableProperty]
   private bool _isSearchBarFocused;

   public RelateContactVm? SelectedRelateContact { get; private set; }

   [ObservableProperty]
   private bool _shouldAggregateConnectionByDay;


   public ContactListVm(
      StorageService storageService,
      IContactDataEnricher dataEnricher,
      ProblemReporterVm problemReporter,
      CallScheduleCalculator callScheduleCalculator,
      IContactService contactService,
      TimeProvider timeProvider,
      INavigationService navigation,
      IDialogService dialogs,
      IBackupPathProvider backupPathProvider,
      IStoragePermission storagePermission,
      ICallService callService,
      IDispatcher dispatcher)
   {
      _storageService = storageService;
      _dataEnricher = dataEnricher;
      _problemReporter = problemReporter;
      _callScheduleCalculator = callScheduleCalculator;
      _contactService = contactService;
      _timeProvider = timeProvider;
      _navigation = navigation;
      _dialogs = dialogs;
      _backupPathProvider = backupPathProvider;
      _storagePermission = storagePermission;
      _callService = callService;

      ObservableContacts = new();
      _contactList = new(ObservableContacts, dispatcher);
   }


   [RelayCommand]
   private void UserStoppedTypingInSearchBar(string _)
   {
      try
      {
         _contactList.ShowFiltered(SearchBarText);
         IsSearchBarFocused = true; // hack: dont loose focus when stopped typing
      }
      finally
      {
      }
   }

   [RelayCommand]
   private async Task RefreshContactList()
   {
      try
      {
         IsListRefreshing = true;
         await LoadContacts();
      }
      finally
      {
         IsListRefreshing = false;
      }
   }

   [RelayCommand]
   private async Task SelectionChanged()
   {
      if (SelectedContact is RelateContactVm contact)
      {
         if (contact.ContactId is not null)
         {
            SelectedRelateContact = contact;

            // ContactDetailsPage resolves its VM from SelectedRelateContact (see MauiProgram),
            // so no navigation parameters are needed here.
            await _navigation.GoToAsync(AppShell.GetRoute<ContactDetailsPage>());
         }
      }

      SelectedContact = null;
   }


   [RelayCommand]
   private void SortByNextCall()
   {
      _contactList.SortByNextCall();
   }


   [RelayCommand]
   private async Task CreateContactsBackup()
   {
      var oneOfResult = await _backupPathProvider.GetContactBackupFilePathAsync();
      await oneOfResult.Match<Task>(async filePath =>
            {
               if (!await _dialogs.ConfirmAsync("Question?",
                      "Do you want to create backup file with all your contacts ?\n\n" +
                      $"Note: File {filePath} will be created. It will NOT be removed when you uninstall this application!",
                      "Yes",
                      "No"))
               {
                  _problemReporter.LogDebug($"Creating backup cancelled");
                  return;
               }

               var maybeError = await _storageService.SaveBackup(_contactList, filePath);
               if (maybeError.HasValue)
               {
                  await ReportBackupErrorAsync(maybeError.Value.Message);
               }
               else
               {
                  _problemReporter.LogDebug($"Backup created to file: '{filePath}'");
               }

            },
            async error =>
            {
               await ReportBackupErrorAsync(error.Message);
            },
            async missingPermission =>
            {
               await PromptForStoragePermissionAsync(missingPermission.Message);
            })
         ;
   }

   private async Task ReportBackupErrorAsync(string message)
   {
      _problemReporter.LogError(message);
      await _dialogs.AlertAsync("Backup error", message, "OK");
   }

   private async Task PromptForStoragePermissionAsync(string permissionName)
   {
      _problemReporter.LogError($"Backup: missing permission: {permissionName}");
      var openSettings = await _dialogs.ConfirmAsync(
         "Permission required",
         "To save or read a backup in the Documents folder, the app "
         + "needs all-files access.\n\n"
         + "Open system settings? After granting the permission, return to the app "
         + "and tap the button again.",
         "Open settings",
         "Cancel");
      if (openSettings)
      {
         _storagePermission.OpenAllFilesAccessSettings();
      }
   }

   [RelayCommand]
   private async Task RestoreContactsFromBackup()
   {
      var oneOfResult = await _backupPathProvider.GetContactBackupFilePathAsync();
      await oneOfResult.Match<Task>(async filePath =>
            {
               if (!_storageService.IsBackupAvailable(filePath))
               {
                  await _dialogs.AlertAsync("Warning", "No backup available", "Ok");
                  return;
               }
               if (!await _dialogs.ConfirmAsync("Question?",
                      "Do you want to restore your contacts from backup ?\n\n" +
                      $"WARNING: This operation will OVERWRITE your current contacts and cannot be undone !",
                      "Yes",
                      "No"))
               {
                  _problemReporter.LogDebug($"Restoring from backup cancelled");
                  return;
               }

               var getContactsOneOf = await _storageService.RestoreFromBackup(filePath);

               await getContactsOneOf.Match<Task>(async result =>
               {
                  if (!result.Any())
                  {
                     _problemReporter.LogDebug($"Contacts from backup not restored -bBackup was empty.");
                     return;
                  }
                  var contacts = result.ToRelateContacts(this, _problemReporter, _callScheduleCalculator, _timeProvider, _navigation, _dialogs, _callService);
                  await ResetContactList(contacts);
                  _problemReporter.LogDebug($"Contacts restored from file '{filePath}'");
               },
                  async error =>
               {
                  await ReportBackupErrorAsync(error.Message);
               });

            },
            async error =>
            {
               await ReportBackupErrorAsync(error.Message);
            },
            async missingPermission =>
            {
               await PromptForStoragePermissionAsync(missingPermission.Message);
            })
         ;
   }

   [RelayCommand]
   private async Task AddContact()
   {
      try
      {
         var oneOf = await _contactService.PickContactAsync();
         await oneOf.Match<Task>(async contact =>
            {
               await AddContactFromPickedContact(contact);
               //var measurement = await Measurements.ExecDuration(async () => await AddContactFromPickedContact(contact));
               //_userMessageReporter.NotifyUser($"Adding took : {measurement}");
            }, async _ =>
            {
               // adding contact cancelled
               await Task.CompletedTask;
            }, async error =>
            {
               _problemReporter.LogError(error.Message);
               await Task.CompletedTask;
            }
         );
      }
      catch (Exception ex)
      {
         _problemReporter.LogError("Unhandled exception while adding contact: " + ex);
      }
   }

   private async Task AddContactFromPickedContact(MySystemContact contact)
   {
      var doesContactExist = _contactList.ContactExists(contact);
      if (doesContactExist)
      {
         await _dialogs.AlertAsync("Warning", "Contact is already in the list", "Ok");
         return;
      }

      var contactVm = new RelateContactVm(this,
         _problemReporter,
         _callScheduleCalculator,
         _timeProvider,
         _navigation,
         _dialogs,
         _callService)
      {
         ContactId = contact.Id,
         ContactDisplayName = contact.DisplayName,
         NoContactPeriodDays = this.NoContactPeriodDays,
         LongCallDurationMinutes = this.ProperCallDurationMinutes,
         ShouldAggregateConnectionByDay = ShouldAggregateConnectionByDay
      };

      await _dataEnricher.EnrichContactWithSystemData(contactVm);
      AddContact(contactVm);
   }

   private void AddContact(RelateContactVm contactVm)
   {
      _contactList.AddContact(contactVm);
   }


   public async Task LoadAsync()
   {
      await LoadContacts();
      LoadDefaultSettings();
   }

   private void LoadDefaultSettings()
   {

      var dto = _storageService.ReadDefaultSettings();
      NoContactPeriodDays = (int)dto.NoContactPeriod.TotalDays;
      ProperCallDurationMinutes = (int)dto.ProperCallDuration.TotalMinutes;
      ShouldAggregateConnectionByDay = dto.ShouldAggregateConnectionByDay;
   }


   private async Task LoadContacts()
   {
      List<RelateContactVm> contacts = _storageService
         .ReadContactsFromStorage()
         .ToRelateContacts(this, _problemReporter, _callScheduleCalculator, _timeProvider, _navigation, _dialogs, _callService);

      await ResetContactList(contacts);
   }

   private async Task ResetContactList(List<RelateContactVm> contacts)
   {
      try
      {
         await _dataEnricher.EnrichContactsWithSystemData(contacts);
      }
      catch (Exception ex)
      {
         _problemReporter.LogError("Unhandled exception while loading contacts: " + ex);
      }

      _contactList.Reset(contacts);
   }

   public void Save()
   {
      _storageService.SaveContacts(_contactList.Items.ToContactsDto());
      _storageService.SaveDefaultSettings(new()
      {
         NoContactPeriod = TimeSpan.FromDays(this.NoContactPeriodDays),
         ProperCallDuration = TimeSpan.FromMinutes(this.ProperCallDurationMinutes),
         ShouldAggregateConnectionByDay = this.ShouldAggregateConnectionByDay
      });
   }

   public bool RemoveContact(string contactId)
   {
      return _contactList.Remove(contactId);
   }
}