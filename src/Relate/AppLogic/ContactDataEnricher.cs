using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.AppLogic.Utils;
using Relate.ViewModels;

namespace Relate.AppLogic;

public class ContactDataEnricher : IContactDataEnricher
{
   private readonly IContactService _contactService;
   private readonly ICallLogService _callLogService;
   private readonly IContactPhotoService _photoService;

   private Lazy<ImageSource> DefaultImage = new(() =>
   {
      return ImageSource.FromFile("user.png");
   });

   public ContactDataEnricher(
      IContactService contactService,
      ICallLogService callLogService,
      IContactPhotoService photoService)
   {
      _contactService = contactService;
      _callLogService = callLogService;
      _photoService = photoService;
   }

   private async Task UpdateContactsWithSystemContactsData(List<RelateContactVm> relateContacts)
   {
      var oneOf = await _contactService.GetContactsAsync();

      await oneOf.Match<Task>(async systemContacts =>
      {
         foreach (var myContact in relateContacts)
         {
            if (myContact.ContactId is null)
            {
               continue;
            }

            var systemContact = systemContacts.SingleOrDefault(x => x.Id == myContact.ContactId);

            await UpdateWithSystemContactData(systemContact, myContact);
         }
      }, error => Task.CompletedTask);
   }

   private async Task UpdateWithSystemContactData(
      MySystemContact? systemContact,
      RelateContactVm myContact)
   {
      if (systemContact is null)
      {
         return;
      }

      myContact.ContactDisplayName = systemContact.DisplayName;
      myContact.PhoneNumbers =
         systemContact.Phones.Select(x => new RelatePhoneNumber(x.PhoneNumber)).ToList();
      myContact.SetPhoneNumbers(
         systemContact.Phones.Select(x => new CallablePhoneNumber(x.PhoneNumber, x.Label)).ToList());

      var contactImageOneOf = await _photoService.GetContactPhoto(myContact.ContactId);
      contactImageOneOf.Switch(image =>
      {
         if (image is not null)
         {
            myContact.ContactImageSource = image;
         }
         else
         {
            myContact.ContactImageSource = DefaultImage.Value;
         }
      }, error => { });

      await Task.CompletedTask;
   }


   private void EnrichContactsWithCallLogs(List<RelateContactVm> relateContacts)
   {
      var (getSystemCallLogDuration, retrieveCallLog) = Measurements.ExecDuration(() =>
      {
         return _callLogService.RetrieveCallLog();
      });


      retrieveCallLog.Switch(
         systemCallLog =>
         {
            var elapsed = Measurements.ExecDuration(() =>
            {
               AddCallsForAllContacts(relateContacts, systemCallLog);
            });
         },
         error =>
         {
         });
   }

   private static void AddCallsForAllContacts(List<RelateContactVm> relateContacts, List<CallLogEntry> systemCallLogs)
   {
      foreach (var relateContact in relateContacts)
      {
         var connections = CallLogMatcher.MatchCalls(relateContact.PhoneNumbers, systemCallLogs);

         // SetCallConnections rebuilds PhoneCalls (respecting the contact's own
         // ShouldAggregateConnectionByDay setting) and recalculates the next-call data.
         relateContact.SetCallConnections(connections);
      }
   }

   public async Task EnrichContactsWithSystemData(IEnumerable<RelateContactVm> contacts)
   {
      var relateContactVms = contacts.ToList();
      await UpdateContactsWithSystemContactsData(relateContactVms);
      EnrichContactsWithCallLogs(relateContactVms);
   }

   // //todo:  fresh does not work yet
   // public async Task RefreshCollection(ObservableCollection<RelateContactVm> observableCollection)
   // {
   //    // hack: for observable collection to change and refresh by removing and re-adding its object
   //    var originalObjects = observableCollection.ToList();
   //    observableCollection.Clear();
   //    await EnrichContactsWithSystemData(originalObjects);
   //
   //    foreach (var relateContactVm in originalObjects)
   //    {
   //       observableCollection.Add(relateContactVm);
   //    }
   // }

   public async Task EnrichContactWithSystemData(RelateContactVm contact)
   {
      var oneOf = await _contactService.GetContactById(contact.ContactId);
      await oneOf.Match<Task>(async (MySystemContact? systemContact) =>
         {
            if (systemContact is null) return;

            await UpdateWithSystemContactData(systemContact, contact); // taskes ~3 seconds
            EnrichContactsWithCallLogs([contact]);
         }, async error =>
         {
            await Task.CompletedTask;
         }
      );
      await Task.CompletedTask;
   }

   public async Task RefreshCollection(IEnumerable<RelateContactVm> relateContacts)
   {
      await EnrichContactsWithSystemData(relateContacts);
   }
}