namespace Relate.AppLogic.Services;

/// <summary>
/// Abstracts the platform check for "can this app write files into a shared/public
/// folder that survives an app reinstall". On Android 11+ that means the special
/// MANAGE_EXTERNAL_STORAGE ("All files access") permission, granted from a system screen.
/// </summary>
public interface IStoragePermission
{
   /// <summary>True when the app may read/write files in public shared storage.</summary>
   bool HasAllFilesAccess();

   /// <summary>Opens the system screen where the user can grant "All files access".</summary>
   void OpenAllFilesAccessSettings();
}

/// <summary>
/// Fallback for platforms without scoped storage (desktop). Normal file IO already works.
/// </summary>
public sealed class NoOpStoragePermission : IStoragePermission
{
   public bool HasAllFilesAccess() => true;

   public void OpenAllFilesAccessSettings()
   {
   }
}
