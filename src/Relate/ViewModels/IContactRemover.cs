namespace Relate.ViewModels;

/// <summary>The slice of <see cref="ContactListVm"/> that a <see cref="RelateContactVm"/> needs.</summary>
public interface IContactRemover
{
   bool RemoveContact(string contactId);
}
