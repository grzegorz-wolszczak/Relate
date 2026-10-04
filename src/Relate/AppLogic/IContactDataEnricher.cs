using Relate.ViewModels;

namespace Relate.AppLogic;

public interface IContactDataEnricher
{
   Task EnrichContactsWithSystemData(IEnumerable<RelateContactVm> contacts);
   Task EnrichContactWithSystemData(RelateContactVm contact);
   Task RefreshCollection(IEnumerable<RelateContactVm> relateContacts);
}
