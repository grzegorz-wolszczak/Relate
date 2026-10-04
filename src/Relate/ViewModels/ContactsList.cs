using System.Collections.ObjectModel;
using Relate.AppLogic.Services.ConditionalCompilation;

namespace Relate.ViewModels;

public class ContactsList
{
   private readonly ObservableCollection<RelateContactVm> _observable;
   private readonly IDispatcher _dispatcher;
   private readonly List<RelateContactVm> _contacts = new();
   public List<RelateContactVm> Items => [.._contacts];

   public ContactsList(ObservableCollection<RelateContactVm> observable, IDispatcher dispatcher)
   {
      _observable = observable;
      _dispatcher = dispatcher;
   }

   public void SortByNextCall()
   {
      // todo: is this correct sorting
      _contacts.Sort((x, y) => x.NextCallInDays - y.NextCallInDays);
      RefreshObservable();
   }

   private void RefreshObservable()
   {
      RefreshObservable(_contacts);
   }

   private void RefreshObservable(IEnumerable<RelateContactVm> contacts)
   {
      // Clear + all Adds must run as one atomic block on the UI thread. Dispatching
      // Clear and each Add as separate, independent Dispatch calls let two overlapping
      // refreshes interleave their queued actions on the dispatcher, which could
      // duplicate rows in the observable collection.
      var snapshot = contacts as IReadOnlyCollection<RelateContactVm> ?? contacts.ToList();
      _dispatcher.Dispatch(() =>
      {
         _observable.Clear();
         foreach (var contact in snapshot)
         {
            _observable.Add(contact);
         }
      });
   }

   public void AddContact(RelateContactVm contactVm)
   {
      _contacts.Add(contactVm);
      _dispatcher.Dispatch(() => { _observable.Add(contactVm); });
   }

   public bool ContactExists(MySystemContact contact)
   {
      return _contacts.Any(x => x.ContactId == contact.Id);
   }

   public void Reset(List<RelateContactVm> contacts)
   {
      _contacts.Clear();
      _contacts.AddRange(contacts);
      RefreshObservable();
   }

   public bool Remove(string contactId)
   {
      var contact = _contacts.FirstOrDefault(x => x.ContactId == contactId);
      var result = contact is not null && _contacts.Remove(contact);
      if (contact is not null)
      {
         _dispatcher.Dispatch(() => { _observable.Remove(contact); });
      }

      return result;
   }

   public void ShowFiltered(string? searchText)
   {
      var finalSearchString = string.IsNullOrWhiteSpace(searchText) ? string.Empty : searchText.Trim();
      if (finalSearchString.Length == 0)
      {
         // update only if observable count was filtered by previous search
         // don't refresh if it is not needed
         if (_observable.Count != _contacts.Count)
         {
            RefreshObservable();
         }

         return;
      }

      var filtered = _contacts.Where(x =>
         x.ContactDisplayName.Contains(finalSearchString, StringComparison.InvariantCultureIgnoreCase));
      RefreshObservable(filtered);
   }
}