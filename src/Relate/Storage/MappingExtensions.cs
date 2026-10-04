using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.Services;
using Relate.ViewModels;

namespace Relate.Storage;

public static class MappingExtensions
{
    public static List<RelateContactVm> ToRelateContacts(
       this List<RelateContactDto> source,
       IContactRemover contactRemover,
       ProblemReporterVm messagesReporter,
       CallScheduleCalculator callScheduleCalculator,
       TimeProvider timeProvider,
       INavigationService navigation,
       IDialogService dialogs,
       ICallService callService)
    {
        return source.Select(x =>
        {
            var vm = new RelateContactVm(
               contactRemover,
               messagesReporter,
               callScheduleCalculator,
               timeProvider,
               navigation,
               dialogs,
               callService)
            {
                ContactId = x.Id,
                ContactDisplayName = x.DisplayName,
                PhoneNumbers = x.PhoneNumbers.Select(p => new RelatePhoneNumber(p)).ToList(),
                LongCallDurationMinutes = (int)x.MinCallDuration.TotalMinutes,
                NoContactPeriodDays = (int)x.MaxNoContactDuration.TotalDays,
                ShouldAggregateConnectionByDay = x.ShouldAggregateConnectionByDay
            };

            foreach (var manualDate in x.ManualContactDates)
            {
                vm.SetManualContactDate(manualDate.Type, manualDate.Date);
            }

            return vm;
        }).ToList();
    }

    public static List<RelateContactDto> ToContactsDto(this IReadOnlyList<RelateContactVm> source)
    {
        return source.Select(x=> new RelateContactDto
        {
            Id = x.ContactId!,
            DisplayName = x.ContactDisplayName,
            PhoneNumbers = x.PhoneNumbers.Select(p=> p.Number).ToList(),
            MinCallDuration = TimeSpan.FromMinutes(x.LongCallDurationMinutes),
            MaxNoContactDuration = TimeSpan.FromDays(x.NoContactPeriodDays),
            ShouldAggregateConnectionByDay = x.ShouldAggregateConnectionByDay,
            ManualContactDates = x.ManualContactEntries
               .Where(e => e.Date.HasValue)
               .Select(e => new ManualContactDateDto { Type = e.Type, Date = e.Date!.Value })
               .ToList()
        }).ToList();
    }



}