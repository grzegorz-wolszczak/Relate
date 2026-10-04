using OneOf;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

//IEnumerable<ContactPhone> phones,

public class MySystemContactPhone
{

   public MySystemContactPhone(string phoneNumber, string label = "")
   {
      PhoneNumber = phoneNumber;
      Label = label;
   }

   public string PhoneNumber { get; set; }
   public string Label { get; set; }


   public override string ToString() => PhoneNumber;
}

public class ContactService : IContactService
{
    public async Task<OneOf<MySystemContact, ActionCancelled, ActionError>> PickContactAsync()
    {
#if WINDOWS
        await Task.CompletedTask;
        return new ActionError("Contact picking is not supported on Windows.");
#elif ANDROID
        Contact? contact = await Contacts.Default.PickContactAsync();
        if (contact is null)
        {
            return ActionCancelled.Instance;
        }

        return new MySystemContact(contact);

#else
    await Task.CompletedTask;
    return new ActionError("Unsupported platform.");
#endif
        // No-op on unsupported platforms
    }


    public async Task<OneOf<List<MySystemContact>, ActionError>> GetContactsAsync()
    {
#if WINDOWS
        await Task.CompletedTask;
        return new ActionError("Retrieving contacts is not supported on Windows.");
#elif ANDROID
       await Task.CompletedTask;
       //return await MultiPlatformContactProvider.GetAllMySystemContacts();
       return  MultiPlatformContactProvider.GetContactsViaAndroidContacts();
#else
    await Task.CompletedTask;
    return new ActionError("Unsupported platform.");
#endif
        // No-op on unsupported platforms
    }


    public async Task<OneOf<MySystemContact?, ActionError>> GetContactById(string? id)
    {
#if WINDOWS
        await Task.CompletedTask;
        return new ActionError("Retrieving contacts is not supported on Windows.");
#elif ANDROID

       //var allContacts = await MultiPlatformContactProvider.GetAllMySystemContacts();
       // todo: optimize this - we can ask for single contact now
       await Task.CompletedTask;
       var contact = MultiPlatformContactProvider.GetContactById(id);

       return contact;
#else
    await Task.CompletedTask;
    return new ActionError("Unsupported platform.");
#endif
       // No-op on unsupported platforms
    }
}