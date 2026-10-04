using OneOf;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public class ContactsPhotoService : IContactPhotoService
{
    public async Task<OneOf<ImageSource?, ActionError>> GetContactPhoto(string contactId)
    {
#if WINDOWS
    await Task.CompletedTask;
    return new ActionError("Contacts photos are not supported on Windows.");
#elif ANDROID
        return await ContactsPhotoProvider.GetContactPhotoAsync(contactId);
#else
    await Task.CompletedTask;
    return new ActionError("Unsupported platform.");
#endif
    }
}