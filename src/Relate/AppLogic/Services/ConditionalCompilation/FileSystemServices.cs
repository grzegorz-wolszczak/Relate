using Relate.AppLogic.Utils.OneOfActions;
using OneOf;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public class FileSystemServices
{
   private const string BackupFolderName = "Relate";
   private const string BackupFileName = "Contacts.backup.json";

   public static async Task<OneOf<string,
      ActionError,
      ActionMissingPermission>> GetContactBackupFilePath()
   {
#if !ANDROID
         await Task.CompletedTask;
         return new ActionError("Operation not supported on current platform");
#else
      // Storage permission is verified by DefaultBackupPathProvider before we get here.
      await Task.CompletedTask;

      // Public shared folders - they survive an app reinstall (unlike app-specific storage).
      var candidates = new[]
      {
         CandidateDir(Android.OS.Environment.DirectoryDocuments),
         CandidateDir(Android.OS.Environment.DirectoryDownloads),
      };

      var dir = BackupDirectoryResolver.Resolve(candidates, PrepareAndProbe);
      if (dir is null)
      {
         return new ActionError(
            "Could not find a writable location for the backup (Documents, Downloads).");
      }

      return Path.Combine(dir, BackupFileName);
#endif
   }

#if ANDROID
   private static string? CandidateDir(string publicDirectoryType)
   {
      var root = Android.OS.Environment
         .GetExternalStoragePublicDirectory(publicDirectoryType)?.AbsolutePath;
      return string.IsNullOrEmpty(root) ? null : Path.Combine(root, BackupFolderName);
   }

   private static bool PrepareAndProbe(string dir)
   {
      Directory.CreateDirectory(dir);
      var probe = Path.Combine(dir, ".relate_probe");
      File.WriteAllText(probe, string.Empty);
      File.Delete(probe);
      return true;
   }
#endif
}
