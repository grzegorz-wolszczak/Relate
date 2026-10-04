using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.Services;
using Relate.ViewModels;

namespace UnitTests.TestSupport;

/// <summary>
/// Single construction point for <see cref="RelateContactVm"/> in tests. Sane deterministic defaults;
/// override only what a test cares about.
/// </summary>
public sealed class RelateContactVmBuilder
{
   private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero));
   private IContactRemover _remover = Mock.Of<IContactRemover>();
   private INavigationService _navigation = Mock.Of<INavigationService>();
   private IDialogService _dialogs = Mock.Of<IDialogService>();
   private ICallService _callService = Mock.Of<ICallService>();
   private string _id = "contact-1";
   private string _name = "Test Contact";
   private string[] _numbers = ["123456789"];
   private int _noContactPeriodDays = 30;
   private int _longCallMinutes = 30;
   private bool _aggregate = true;
   private CallConnection[]? _connections;
   private readonly List<(ContactType Type, DateTimeOffset Date)> _manualContactDates = new();

   public FakeTimeProvider Clock => _clock;

   public RelateContactVmBuilder Now(DateTimeOffset now)
   {
      _clock.SetUtcNow(now);
      return this;
   }

   public RelateContactVmBuilder Named(string name)
   {
      _name = name;
      return this;
   }

   public RelateContactVmBuilder WithId(string? id)
   {
      _id = id!;
      return this;
   }

   public RelateContactVmBuilder WithNumbers(params string[] numbers)
   {
      _numbers = numbers;
      return this;
   }

   public RelateContactVmBuilder WithSettings(int noContactPeriodDays, int longCallMinutes, bool aggregate)
   {
      _noContactPeriodDays = noContactPeriodDays;
      _longCallMinutes = longCallMinutes;
      _aggregate = aggregate;
      return this;
   }

   public RelateContactVmBuilder WithManualContactDate(ContactType type, DateTimeOffset date)
   {
      _manualContactDates.Add((type, date));
      return this;
   }

   public RelateContactVmBuilder WithConnections(params CallConnection[] connections)
   {
      _connections = connections;
      return this;
   }

   public RelateContactVmBuilder WithRemover(IContactRemover remover)
   {
      _remover = remover;
      return this;
   }

   public RelateContactVmBuilder WithNavigation(INavigationService navigation)
   {
      _navigation = navigation;
      return this;
   }

   public RelateContactVmBuilder WithDialogs(IDialogService dialogs)
   {
      _dialogs = dialogs;
      return this;
   }

   public RelateContactVmBuilder WithCallService(ICallService callService)
   {
      _callService = callService;
      return this;
   }

   public RelateContactVmBuilder WithNoPhoneNumbers()
   {
      _numbers = [];
      return this;
   }

   public RelateContactVm Build()
   {
      var reporter = new ProblemReporterVm(NullLogger<ProblemReporterVm>.Instance, new ImmediateDispatcher());

      var vm = new RelateContactVm(
         _remover, reporter, new CallScheduleCalculator(), _clock, _navigation, _dialogs, _callService)
      {
         ContactId = _id,
         ContactDisplayName = _name,
         PhoneNumbers = _numbers.Select(n => new Relate.AppLogic.RelatePhoneNumber(n)).ToList(),
         NoContactPeriodDays = _noContactPeriodDays,
         LongCallDurationMinutes = _longCallMinutes,
         ShouldAggregateConnectionByDay = _aggregate,
      };
      vm.SetPhoneNumbers(_numbers.Select(n => new CallablePhoneNumber(n, "Phone")).ToList());

      foreach (var (type, date) in _manualContactDates)
      {
         vm.SetManualContactDate(type, date);
      }

      if (_connections is not null)
      {
         vm.SetCallConnections(_connections.ToList());
      }

      return vm;
   }
}
