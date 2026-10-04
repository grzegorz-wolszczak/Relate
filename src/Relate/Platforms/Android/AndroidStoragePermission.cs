using Android.Content;
using Android.OS;
using Android.Provider;
using Relate.AppLogic.Services;
using Uri = Android.Net.Uri;

namespace Relate.Platforms.Android;

/// <summary>
/// Android implementation of <see cref="IStoragePermission"/>. On API 30+ the app can only
/// write into public shared folders (Documents/Downloads) with MANAGE_EXTERNAL_STORAGE
/// ("All files access"), which the user grants on a dedicated system screen.
/// </summary>
public sealed class AndroidStoragePermission : IStoragePermission
{
   public bool HasAllFilesAccess()
   {
      if (OperatingSystem.IsAndroidVersionAtLeast(30))
      {
         return global::Android.OS.Environment.IsExternalStorageManager;
      }

      // API < 30: legacy WRITE_EXTERNAL_STORAGE runtime permission is enough.
      return Permissions.CheckStatusAsync<Permissions.StorageWrite>()
         .GetAwaiter().GetResult() == PermissionStatus.Granted;
   }

   public void OpenAllFilesAccessSettings()
   {
      var context = Platform.AppContext;

      if (OperatingSystem.IsAndroidVersionAtLeast(30))
      {
         var packageUri = Uri.Parse("package:" + context.PackageName);
         var perAppIntent = new Intent(
            Settings.ActionManageAppAllFilesAccessPermission, packageUri);
         perAppIntent.AddFlags(ActivityFlags.NewTask);

         if (perAppIntent.ResolveActivity(context.PackageManager!) != null)
         {
            context.StartActivity(perAppIntent);
            return;
         }

         var listIntent = new Intent(Settings.ActionManageAllFilesAccessPermission);
         listIntent.AddFlags(ActivityFlags.NewTask);
         if (listIntent.ResolveActivity(context.PackageManager!) != null)
         {
            context.StartActivity(listIntent);
            return;
         }
      }

      // Fallback: this app's details screen, from where "Permissions" is reachable.
      var detailsIntent = new Intent(
         Settings.ActionApplicationDetailsSettings,
         Uri.Parse("package:" + context.PackageName));
      detailsIntent.AddFlags(ActivityFlags.NewTask);
      context.StartActivity(detailsIntent);
   }
}
