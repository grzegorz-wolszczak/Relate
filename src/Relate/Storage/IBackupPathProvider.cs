using OneOf;
using Relate.AppLogic.Services;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.Storage;

public interface IBackupPathProvider
{
   Task<OneOf<string, ActionError, ActionMissingPermission>> GetContactBackupFilePathAsync();
}

public sealed class DefaultBackupPathProvider : IBackupPathProvider
{
   private readonly IStoragePermission _storagePermission;

   public DefaultBackupPathProvider(IStoragePermission storagePermission)
   {
      _storagePermission = storagePermission;
   }

   public Task<OneOf<string, ActionError, ActionMissingPermission>> GetContactBackupFilePathAsync()
   {
      if (!_storagePermission.HasAllFilesAccess())
      {
         return Task.FromResult<OneOf<string, ActionError, ActionMissingPermission>>(
            new ActionMissingPermission("All files access"));
      }

      return FileSystemServices.GetContactBackupFilePath();
   }
}
