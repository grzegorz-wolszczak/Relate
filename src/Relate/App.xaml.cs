using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Relate.AppLogic.Services;
using Relate.ViewModels;

namespace Relate;

public partial class App : Application
{
   private readonly ContactListVm _contactListVm;

   public App(AppShell shell, ContactListVm contactListVm)
   {
      _contactListVm = contactListVm;
      // Always use light colors (light backgrounds, dark text), regardless of
      // the device's system theme - must be set before InitializeComponent so
      // AppThemeBinding resources in Styles.xaml resolve to their Light branch.
      UserAppTheme = AppTheme.Light;
      InitializeComponent();
      MainPage = shell;
   }

   protected override Window CreateWindow(IActivationState? activationState)
   {
      var window = base.CreateWindow(activationState);

      // pretend we're on a tablet by making the window taller than it is wide
      double newWidth = 600;
      double newHeight = 1000;
      window.Width = newWidth;
      window.Height = newHeight;

      return window;
   }

   protected override void OnResume()
   {
      Trace.WriteLine("\n*** App OnResume ***\n");
      base.OnResume();
   }

   protected override async void OnStart()
   {
      if (!await AppPermissions.CheckRequiredPermissions())
      {
         await Toast.Make("Not all permissions were accepted. " +
                          "Application will close.\n" +
                          "Allow app permissions in system settings.", duration: ToastDuration.Long).Show();
         Application.Current?.Quit();
         return;
      }

      await AppPermissions.RequestOptionalPermissionsAsync();

      Trace.WriteLine("\n*** App OnStart ***\n");
      base.OnStart();
      await _contactListVm.LoadAsync(); // todo: fix this, it should be awaited properly
   }

   protected override void OnSleep()
   {
      Trace.WriteLine("\n*** App OnSleep ***\n");
      base.OnSleep();
      _contactListVm.Save();
   }
}