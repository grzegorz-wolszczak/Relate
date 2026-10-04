using OneOf;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public interface IContactPhotoService
{
   Task<OneOf<ImageSource?, ActionError>> GetContactPhoto(string contactId);
}
