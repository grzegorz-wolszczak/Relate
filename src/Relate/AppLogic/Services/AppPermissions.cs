namespace Relate.AppLogic.Services;

#if ANDROID
/// <summary>Wraps just READ_CALL_LOG - Permissions.Phone bundles it together with
/// CALL_PHONE and other phone-state permissions as one atomic status, which would
/// make it impossible to require call-log access while treating calling as optional.</summary>
public class ReadCallLogPermission : Permissions.BasePlatformPermission
{
   public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
      new[] { (global::Android.Manifest.Permission.ReadCallLog, true) };
}

/// <summary>Wraps just CALL_PHONE, isolated from Permissions.Phone for the same reason.</summary>
public class CallPhonePermission : Permissions.BasePlatformPermission
{
   public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
      new[] { (global::Android.Manifest.Permission.CallPhone, true) };
}
#endif

public static class AppPermissions
{
    private static async Task<PermissionStatus> CheckPermissions<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted){
            status = await Permissions.RequestAsync<TPermission>();
        }

        return status;
    }

    private static bool IsGranted(PermissionStatus status)
    {
        return status == PermissionStatus.Granted || status == PermissionStatus.Limited;
    }

    /// <summary>Permissions the app cannot function without - declining any of these closes the app.</summary>
    public static async Task<bool> CheckRequiredPermissions()
    {
       var granted =
          IsGranted(await CheckPermissions<Permissions.ContactsRead>())
          && IsGranted(await CheckPermissions<Permissions.StorageRead>())
          && IsGranted(await CheckPermissions<Permissions.StorageWrite>());

#if ANDROID
       granted = granted && IsGranted(await CheckPermissions<ReadCallLogPermission>());
#endif

       return granted;
    }

    /// <summary>Nice-to-have permissions for optional features (calling). Requested at startup
    /// as a courtesy so the OS dialog doesn't surprise the user mid-task; declining does NOT
    /// block the app - ICallService re-asks (and offers Settings) when the feature is actually used.</summary>
    public static async Task RequestOptionalPermissionsAsync()
    {
#if ANDROID
       await CheckPermissions<CallPhonePermission>();
#else
       await Task.CompletedTask;
#endif
    }
}
