
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.AppLogic.Utils;
using Relate.Services;

namespace Relate.ViewModels;

// partial class to allow code generation for CommunityToolkit.Mvvm
public partial class RelateContactVm : BaseVm
{
   private readonly IContactRemover _contactRemover;
   private readonly ProblemReporterVm _problemReporter;
   private readonly CallScheduleCalculator _callScheduleCalculator;
   private readonly TimeProvider _timeProvider;
   private readonly INavigationService _navigation;
   private readonly IDialogService _dialogs;
   private readonly ICallService _callService;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(ContactImageColor))]
   string? _contactId;

   [ObservableProperty]
   private ImageSource? _contactImageSource;

   [ObservableProperty]
   private DateTimeOffset? _nextCallDateTime;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(NextCallInDaysDisplay))]
   private int _nextCallInDays; // can be less than zero, meaning we missed scheduled contact

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(LastContactCardDisplay))]
   [NotifyPropertyChangedFor(nameof(NextCallInDaysDisplay))]
   private DateTimeOffset? _lastContactDateTime;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(LastContactCardDisplay))]
   private ContactType? _lastContactType;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(LastContactCardDisplay))]
   private TimeSpan? _lastContactCallDuration;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(ContactImageColor))]
   private string _contactDisplayName;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(NextCallInDaysDisplay))]
   private int _noContactPeriodDays;

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(NextCallInDaysDisplay))]
   private int _longCallDurationMinutes;

   [ObservableProperty]
   private bool _shouldAggregateConnectionByDay = false;

   private List<CallConnection> _callConnections = new();
   private PhoneCalls _phoneCalls = new(new List<PhoneCall>());


   // no need for observable property
   public List<RelatePhoneNumber> PhoneNumbers { get; set; } = new();
   public string NextCallInDaysDisplay => CallDisplayHelper.GetNextContactInDaysDisplayDetails(NextCallInDays, LastContactDateTime.HasValue);
   public string LastContactCardDisplay => CallDisplayHelper.GetLastContactCardDisplay(
      LastContactDateTime, LastContactType, LastContactCallDuration, _timeProvider.GetLocalNow(), _timeProvider.LocalTimeZone);
   public Color ContactImageColor => PseudoRandomBackgroundColor.Get(ContactDisplayName, ContactId);

   // one entry per manually-dated contact type (see ManualContactTypeCatalog) - fixed for the
   // lifetime of this VM, populated from storage via SetManualContactDate.
   public IReadOnlyList<ManualContactEntryVm> ManualContactEntries { get; }

   private List<CallablePhoneNumber> _callablePhoneNumbers = new();
   public IReadOnlyList<CallablePhoneNumber> CallablePhoneNumbers => _callablePhoneNumbers;
   public bool HasPhoneNumbers => _callablePhoneNumbers.Count > 0;
   public Color PhoneIconBackgroundColor => HasPhoneNumbers ? SoftPalette.AccentTeal : SoftPalette.CardStroke;

   public RelateContactVm(
      IContactRemover contactRemover,
      ProblemReporterVm problemReporter,
      CallScheduleCalculator callScheduleCalculator,
      TimeProvider timeProvider,
      INavigationService navigation,
      IDialogService dialogs,
      ICallService callService)
   {
      _contactRemover = contactRemover ?? throw new ArgumentNullException(nameof(contactRemover));
      _problemReporter = problemReporter;
      _callScheduleCalculator = callScheduleCalculator;
      _timeProvider = timeProvider;
      _navigation = navigation;
      _dialogs = dialogs;
      _callService = callService;

      ManualContactEntries = ManualContactTypeCatalog.All
         .Select(c => new ManualContactEntryVm(c.Type, c.Label, onChanged: UpdateContactWithRecalculatedNextCall))
         .ToList();
   }

   public DateTimeOffset? GetManualContactDate(ContactType type) =>
      ManualContactEntries.FirstOrDefault(e => e.Type == type)?.Date;

   public void SetManualContactDate(ContactType type, DateTimeOffset? date)
   {
      var entry = ManualContactEntries.FirstOrDefault(e => e.Type == type);
      if (entry is not null)
      {
         entry.Date = date;
      }
   }

   // mirrors SetCallConnections: mutates private state and raises the notifications the UI needs
   public void SetPhoneNumbers(List<CallablePhoneNumber> phoneNumbers)
   {
      _callablePhoneNumbers = phoneNumbers;
      OnPropertyChanged(nameof(CallablePhoneNumbers));
      OnPropertyChanged(nameof(HasPhoneNumbers));
      OnPropertyChanged(nameof(PhoneIconBackgroundColor));
   }

   [RelayCommand]
   private async Task RemoveContact()
   {
      if (ContactId is null)
      {
         _problemReporter.LogError("Internal Error: No contact id available");
         return;
      }

      if (await _dialogs.ConfirmAsync("Question?",
             "Are you sure you want to remove this contact ?",
             "Yes",
             "No"))
      {
         var removed = _contactRemover.RemoveContact(ContactId);
         if (removed)
         {
            // we must 'navigate back' after delete, otherwise we will see <- arrow on the main view !
            await HandleBackButton();
         }
      }
   }

   [RelayCommand]
   private async Task HandleBackButton()
   {
      await _navigation.GoBackAsync();
   }

   [RelayCommand]
   private async Task CallContact()
   {
      if (!HasPhoneNumbers)
      {
         await _dialogs.AlertAsync(
            "No phone number",
            "This contact has no saved phone number. Add a phone number to be able to call.",
            "OK");
         return;
      }

      // calling is an optional permission: it may never have been granted (declined at
      // startup, or the user hadn't been asked yet on this install) or later revoked.
      if (!await _callService.EnsureCallPermissionAsync())
      {
         var openSettings = await _dialogs.ConfirmAsync(
            "Permission required",
            "Calling from the app requires permission to place phone calls.\n\n"
            + "Open system settings? After granting the permission, return to the app and try again.",
            "Open settings",
            "Cancel");
         if (openSettings)
         {
            _callService.OpenCallPermissionSettings();
         }
         return; // call action cancelled either way
      }

      var options = _callablePhoneNumbers.Select(p => $"{p.Label}: {p.Number}").ToArray();
      var choice = await _dialogs.PickOptionAsync("Call", "Cancel", options);
      if (choice is null)
      {
         return; // cancelled
      }

      var index = Array.IndexOf(options, choice);
      if (index < 0)
      {
         return;
      }

      _callService.PlaceCall(_callablePhoneNumbers[index].Number);
   }


   // all the change update commands
   partial void OnNoContactPeriodDaysChanged(int value)
   {
      UpdateContactWithRecalculatedNextCall();
   }

   partial void OnLongCallDurationMinutesChanged(int value)
   {
      UpdateContactWithRecalculatedNextCall();
   }

   partial void OnShouldAggregateConnectionByDayChanged(bool value)
   {
      RebuildPhoneCalls();
      UpdateContactWithRecalculatedNextCall();
   }

   private void UpdateContactWithRecalculatedNextCall()
   {
      _callScheduleCalculator
         .RecalculateCallsRelatedData(
            _phoneCalls,
            LongCallDurationMinutes,
            NoContactPeriodDays,
            ManualContactEntries.Select(e => (e.Type, e.Date)).ToList(),
            _timeProvider.GetLocalNow())
         .CopyValuesTo(this);
   }

   // rebuilds PhoneCalls honouring this contact's own ShouldAggregateConnectionByDay
   // setting and recalculates the next-call data
   public void SetCallConnections(List<CallConnection> callConnections)
   {
      _callConnections = callConnections;
      RebuildPhoneCalls();
      UpdateContactWithRecalculatedNextCall();
   }

   private void RebuildPhoneCalls()
   {
      _phoneCalls = PhoneCallProcessor.ProcessCallConnections(_callConnections, ShouldAggregateConnectionByDay);
   }
}