namespace Relate.AppLogic.Services.ConditionalCompilation;

public static class MultiPlatformContactProvider
{
   public static async Task<List<MySystemContact>> GetAllMySystemContacts()
   {
      var allAsync = (await Contacts.Default.GetAllAsync());
      var mySystemContacts = allAsync.Select(x=>new MySystemContact(x)).ToList();
      return mySystemContacts;
   }

   public static List<MySystemContact> GetContactsViaAndroidContacts()
   {
      #if ANDROID
      var allAsync = ContactsProvider.LoadContacts();
      var mySystemContacts = allAsync.Select(x=>new MySystemContact(x)).ToList();
      return mySystemContacts;
   #else
      return [];
#endif
   }


   public static MySystemContact? GetContactById(string?id)
   {
#if ANDROID
      var androidContact = ContactsProvider.GetContactById(id);
      if (androidContact is null) return null;
      return new(androidContact);
#else
      return null;
#endif
   }

}