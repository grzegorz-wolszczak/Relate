using OneOf;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.AppLogic.Utils.OneOfActions;
using Relate.ViewModels;
using static UnitTests.TestSupport.Build;

namespace UnitTests.AppLogic;

public class ContactDataEnricherTests
{
   private readonly Mock<IContactService> _contacts = new();
   private readonly Mock<ICallLogService> _callLog = new();
   private readonly Mock<IContactPhotoService> _photos = new();
   private readonly ContactDataEnricher _sut;

   private static OneOf<List<MySystemContact>, ActionError> Contacts(params MySystemContact[] c) => c.ToList();
   private static OneOf<List<CallLogEntry>, ActionError> Calls(params CallLogEntry[] c) => c.ToList();
   private static OneOf<MySystemContact?, ActionError> ById(MySystemContact c) => c;

   public ContactDataEnricherTests()
   {
      // Arrange (shared): platform services succeed with empty results by default
      _contacts.Setup(c => c.GetContactsAsync()).ReturnsAsync(Contacts());
      _callLog.Setup(c => c.RetrieveCallLog()).Returns(Calls());
      _photos.Setup(p => p.GetContactPhoto(It.IsAny<string>()))
         .ReturnsAsync((OneOf<ImageSource?, ActionError>)new ActionError("no photo"));

      _sut = new ContactDataEnricher(_contacts.Object, _callLog.Object, _photos.Object);
   }

   private static RelateContactVm Vm(string id, string name, params string[] numbers) =>
      new RelateContactVmBuilder().Now(At.Utc(2026, 8, 27, 12))
         .WithId(id).Named(name).WithNumbers(numbers)
         .WithSettings(noContactPeriodDays: 30, longCallMinutes: 30, aggregate: true)
         .Build();

   [Fact]
   public async Task Refreshes_display_name_and_phone_numbers_from_the_system_contact()
   {
      // Arrange
      var vm = Vm("c1", "Old Name", "999");
      _contacts.Setup(c => c.GetContactsAsync())
         .ReturnsAsync(Contacts(SystemContact("c1", "New Name", "123456789")));

      // Act
      await _sut.EnrichContactsWithSystemData([vm]);

      // Assert
      vm.ContactDisplayName.Should().Be("New Name");
      vm.PhoneNumbers.Select(p => p.Number).Should().Equal("123456789");
   }

   [Fact]
   public async Task Attaches_matching_call_log_entries_and_recalculates_the_schedule()
   {
      // Arrange
      var vm = Vm("c1", "n", "999");
      _contacts.Setup(c => c.GetContactsAsync())
         .ReturnsAsync(Contacts(SystemContact("c1", "n", "123456789")));
      _callLog.Setup(c => c.RetrieveCallLog())
         .Returns(Calls(LogEntry("123456789", At.Utc(2026, 8, 20), 45 * 60)));

      // Act
      await _sut.EnrichContactsWithSystemData([vm]);

      // Assert
      vm.NextCallInDays.Should().Be(23);
   }

   [Fact]
   public async Task A_contact_service_error_leaves_the_view_models_untouched()
   {
      // Arrange
      var vm = Vm("c1", "Original", "999");
      _contacts.Setup(c => c.GetContactsAsync())
         .ReturnsAsync((OneOf<List<MySystemContact>, ActionError>)new ActionError("no permission"));

      // Act
      await _sut.EnrichContactsWithSystemData([vm]);

      // Assert
      vm.ContactDisplayName.Should().Be("Original");
   }

   [Fact]
   public async Task A_call_log_error_does_not_throw()
   {
      // Arrange
      var vm = Vm("c1", "n", "999");
      _callLog.Setup(c => c.RetrieveCallLog())
         .Returns((OneOf<List<CallLogEntry>, ActionError>)new ActionError("denied"));

      // Act
      var act = () => _sut.EnrichContactsWithSystemData([vm]);

      // Assert
      await act.Should().NotThrowAsync();
   }

   [Fact]
   public async Task A_contact_with_no_system_match_keeps_its_current_data()
   {
      // Arrange
      var vm = Vm("missing", "Keep Me", "999");
      _contacts.Setup(c => c.GetContactsAsync())
         .ReturnsAsync(Contacts(SystemContact("other", "Someone Else", "111")));

      // Act
      await _sut.EnrichContactsWithSystemData([vm]);

      // Assert
      vm.ContactDisplayName.Should().Be("Keep Me");
   }

   [Fact]
   public async Task EnrichContactWithSystemData_uses_get_contact_by_id()
   {
      // Arrange
      var vm = Vm("c1", "Old", "999");
      _contacts.Setup(c => c.GetContactById("c1"))
         .ReturnsAsync(ById(SystemContact("c1", "Fresh", "123456789")));

      // Act
      await _sut.EnrichContactWithSystemData(vm);

      // Assert
      vm.ContactDisplayName.Should().Be("Fresh");
   }
}
