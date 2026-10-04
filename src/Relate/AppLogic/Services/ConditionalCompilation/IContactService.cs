using OneOf;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public interface IContactService
{
   Task<OneOf<MySystemContact, ActionCancelled, ActionError>> PickContactAsync();
   Task<OneOf<List<MySystemContact>, ActionError>> GetContactsAsync();
   Task<OneOf<MySystemContact?, ActionError>> GetContactById(string? id);
}
