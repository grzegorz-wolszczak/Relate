namespace Relate.AppLogic.Services;

/// <summary>
/// Abstracts the defensive runtime check for the call permission (it can be revoked via
/// system Settings after Permissions.Phone was already granted at startup - see
/// AppPermissions) and placing a direct phone call.
/// </summary>
public interface ICallService
{
   /// <summary>Ensures the call permission is granted, requesting it if needed. Never throws.</summary>
   Task<bool> EnsureCallPermissionAsync();

   /// <summary>Opens the system screen where the user can manually grant the call permission
   /// (used when a fresh runtime request no longer shows a dialog, e.g. after "don't ask again").</summary>
   void OpenCallPermissionSettings();

   /// <summary>Places a direct call (Android Intent.ActionCall / tel: URI). No-op if unsupported.</summary>
   void PlaceCall(string phoneNumber);
}

/// <summary>Fallback for platforms without telephony (desktop). Never grants, never dials.</summary>
public sealed class NoOpCallService : ICallService
{
   public Task<bool> EnsureCallPermissionAsync() => Task.FromResult(false);

   public void OpenCallPermissionSettings()
   {
   }

   public void PlaceCall(string phoneNumber)
   {
   }
}
