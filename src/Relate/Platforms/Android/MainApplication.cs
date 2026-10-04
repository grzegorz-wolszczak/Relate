

using Android.App;
using Android.Runtime;

//using global :: Android.App.Application.Context.ContentResolver.Query;
[assembly: UsesPermission(Android.Manifest.Permission.ReadContacts)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadCallLog)]
[assembly: UsesPermission(Android.Manifest.Permission.ReadPhoneState)]
namespace Relate;



[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {

    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


}