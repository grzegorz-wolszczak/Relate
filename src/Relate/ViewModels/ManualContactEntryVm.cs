using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;

namespace Relate.ViewModels;

// One instance per manually-dated contact type (see ManualContactTypeCatalog). Kept as its own
// bindable object - rather than a set of per-type properties/methods on RelateContactVm - so a
// details-page card can bind directly to plain properties on this object (BindingContext = the
// entry). CommunityToolkit.Maui.Markup's .Bind(getter: ...) only supports a simple property
// access expression in the lambda, not a method call with a captured parameter, so a single
// shared "GetManualDate(ContactType)" method on RelateContactVm could not be bound directly.
public partial class ManualContactEntryVm : ObservableObject
{
   private readonly Action _onChanged;

   public ContactType Type { get; }
   public string Label { get; }

   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(SummaryDisplay))]
   [NotifyPropertyChangedFor(nameof(DateOrToday))]
   private DateTimeOffset? _date;

   // plain DateTime for two-way binding to DatePicker.Date (which is non-nullable)
   public DateTime DateOrToday => Date?.LocalDateTime.Date ?? DateTime.Today;

   public string SummaryDisplay => CallDisplayHelper.GetManualContactDisplay(Label, Date);

   public ManualContactEntryVm(ContactType type, string label, Action onChanged)
   {
      Type = type;
      Label = label;
      _onChanged = onChanged;
   }

   partial void OnDateChanged(DateTimeOffset? value)
   {
      _onChanged();
   }

   [RelayCommand]
   private void Clear()
   {
      Date = null;
   }
}
