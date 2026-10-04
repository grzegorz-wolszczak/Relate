using Relate.ViewModels;

namespace Relate.AppLogic.CallProcessing;

public record NextCallRecalculation
{
   public required DateTimeOffset NextCallDateTime { get; init; }
   public required int NextCallInDays { get; init; }
   public DateTimeOffset? LastContactDateTime { get; init; }
   public ContactType? LastContactType { get; init; }
   public TimeSpan? LastContactCallDuration { get; init; }
}

public static class NextCallRecalculationExtensions
{
   public static void CopyValuesTo(this NextCallRecalculation data, RelateContactVm vm)
   {
      vm.NextCallInDays = data.NextCallInDays;
      vm.NextCallDateTime = data.NextCallDateTime;
      vm.LastContactDateTime = data.LastContactDateTime;
      vm.LastContactType = data.LastContactType;
      vm.LastContactCallDuration = data.LastContactCallDuration;
   }
}