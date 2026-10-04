using Relate.Pages;

namespace Relate;

public class AppShell: Shell
{
    public AppShell(ContactListPage contactListPage)
    {
        Items.Add(contactListPage);
        CreateRoutes();

    }

    public static string GetRoute<T>() where T : ContentPage
    {
        if (typeof(T) == typeof(ContactDetailsPage))
        {
            return $"//{nameof(ContactListPage)}/{nameof(ContactDetailsPage)}";
            //return $"//{nameof(ContactDetailsPage)}";
        }

        if (typeof(T) == typeof(ContactListPage))
        {
            return $"//{nameof(ContactListPage)}";
        }
        throw new NotImplementedException($"No route defined for {typeof(T).FullName}");
    }

    static void CreateRoutes()
    {
        Routing.RegisterRoute(GetRoute<ContactListPage>(), typeof(ContactListPage));
        Routing.RegisterRoute(GetRoute<ContactDetailsPage>(), typeof(ContactDetailsPage));
    }
}