using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Services;
using Relate.AppLogic.Services.ConditionalCompilation;
using Relate.Pages;
using Relate.Services;
using Relate.Storage;
using Relate.ViewModels;
using Syncfusion.Maui.Toolkit.Hosting;

namespace Relate;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ContactListVm>();
        builder.Services.AddSingleton<IContactRemover>(sp => sp.GetRequiredService<ContactListVm>());
        builder.Services.AddSingleton<ICallLogService, CallLogService>();
        builder.Services.AddSingleton<IContactPhotoService, ContactsPhotoService>();
        builder.Services.AddSingleton<IContactDataEnricher, ContactDataEnricher>();
        builder.Services.AddSingleton<CallScheduleCalculator>(ctx =>
        {
           return new CallScheduleCalculator();
        });
        builder.Services.AddSingleton<IContactService, ContactService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();
        builder.Services.AddSingleton<IFileStore, PhysicalFileStore>();
#if ANDROID
        builder.Services.AddSingleton<IStoragePermission, Platforms.Android.AndroidStoragePermission>();
        builder.Services.AddSingleton<ICallService, Platforms.Android.AndroidCallService>();
#else
        builder.Services.AddSingleton<IStoragePermission, NoOpStoragePermission>();
        builder.Services.AddSingleton<ICallService, NoOpCallService>();
#endif
        builder.Services.AddSingleton<IBackupPathProvider, DefaultBackupPathProvider>();
        builder.Services.AddSingleton<RelateContactVm>();
        builder.Services.AddSingleton<StorageService>();
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();
        builder.Services.AddTransient<ContactListPage>();
        builder.Services.AddTransient<ContactDetailsPage>(ctx =>
        {
           var listVm = ctx.GetRequiredService<ContactListVm>();
           var relateContactViewModel = listVm.SelectedRelateContact ?? ctx.GetRequiredService<RelateContactVm>();
           return new(relateContactViewModel);
        });
        builder.Services.AddSingleton<ProblemReporterVm>();
        return builder.Build();
    }
}