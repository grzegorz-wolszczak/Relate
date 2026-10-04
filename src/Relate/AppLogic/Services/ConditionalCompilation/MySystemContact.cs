using Relate.AppLogic;
using Relate.AppLogic.Models;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public record MySystemContact
{
   public string Id { get; private init; }
   public string DisplayName { get; private init; }
   public List<MySystemContactPhone> Phones { get; private init; }
   public MySystemContact(Contact contact)
   {
      DisplayName = contact.DisplayName;
      Id = contact.Id;
      Phones = DeduplicatePhones(contact.Phones.Select(p => new MySystemContactPhone(p.PhoneNumber)));
   }

   public MySystemContact(AndroidContact contact)
   {
      DisplayName = contact.DisplayName;
      Id = contact.Id;
      Phones = DeduplicatePhones(contact.Phones.Select(p => new MySystemContactPhone(p.Number, p.Label)));
   }

   // Android's contacts content provider can return the same number more than once for one
   // contact - e.g. it's synced from two accounts (Google + phone/SIM storage), each
   // contributing its own raw contact row with the same "Mobile" entry. The duplicate rows can
   // even format the digits slightly differently (spaces, dashes, a "+48" prefix), so compare by
   // normalized digits (RelatePhoneNumber's normalization) rather than the raw string, keeping
   // the first occurrence's label.
   private static List<MySystemContactPhone> DeduplicatePhones(IEnumerable<MySystemContactPhone> phones) =>
      phones.DistinctBy(p => new RelatePhoneNumber(p.PhoneNumber).Number).ToList();
}