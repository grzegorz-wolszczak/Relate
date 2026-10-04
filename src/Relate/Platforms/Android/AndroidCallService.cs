using Android.Content;
using Relate.AppLogic.Services;
using Uri = Android.Net.Uri;

namespace Relate.Platforms.Android;

public sealed class AndroidCallService : ICallService
{
   public async Task<bool> EnsureCallPermissionAsync()
   {
      var status = await Permissions.CheckStatusAsync<CallPhonePermission>();
      if (status != PermissionStatus.Granted)
      {
         status = await Permissions.RequestAsync<CallPhonePermission>();
      }

      return status is PermissionStatus.Granted or PermissionStatus.Limited;
   }

   public void OpenCallPermissionSettings() => AppInfo.Current.ShowSettingsUI();

   public void PlaceCall(string phoneNumber)
   {
      var context = Platform.AppContext;
      var intent = new Intent(Intent.ActionCall, Uri.Parse($"tel:{phoneNumber}"));
      intent.AddFlags(ActivityFlags.NewTask);
      context.StartActivity(intent);
   }
}
