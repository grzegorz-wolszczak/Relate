using Android.Content;
using Android.Provider;
using Relate.AppLogic.Utils.OneOfActions;
using OneOf;

public static class ContactsPhotoProvider
{
    public static async Task<OneOf<ImageSource?, ActionError>> GetContactPhotoAsync(string contactId)
    {
#if ANDROID
        var context = Platform.AppContext;
        try
        {
            var uri = ContentUris.WithAppendedId(ContactsContract.Contacts.ContentUri, long.Parse(contactId));
            var inputStream = ContactsContract.Contacts.OpenContactPhotoInputStream(context.ContentResolver, uri, true);
            if (inputStream == null)
            {
                // todo - maybe return custom "no photo" image or avatar with initials
                return (ImageSource?)null!;
            }

            using (var memoryStream = new MemoryStream())
            {
                await inputStream.CopyToAsync(memoryStream);
                var contactPhotoAsync = memoryStream.ToArray();

                var imageSource = ImageSource.FromStream(() => new MemoryStream(contactPhotoAsync));
                return imageSource;
            }
        }
        catch (Exception ex)
        {
            return new ActionError($"Error retrieving contact photo: {ex.Message}");
        }


#else
     await Task.CompletedTask;
     return null; // No implementation for other platforms
#endif
    }
}