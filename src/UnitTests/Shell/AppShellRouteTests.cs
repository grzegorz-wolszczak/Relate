using Relate;
using Relate.Pages;

namespace UnitTests.Shell;

public class AppShellRouteTests
{
   [Fact]
   public void Contact_list_route()
   {
      // Act
      var route = AppShell.GetRoute<ContactListPage>();

      // Assert
      route.Should().Be("//ContactListPage");
   }

   [Fact]
   public void Contact_details_route_is_nested_under_the_list()
   {
      // Act
      var route = AppShell.GetRoute<ContactDetailsPage>();

      // Assert
      route.Should().Be("//ContactListPage/ContactDetailsPage");
   }

   [Fact]
   public void An_unknown_page_type_has_no_route()
   {
      // Act
      var act = () => AppShell.GetRoute<UnknownPage>();

      // Assert
      act.Should().Throw<NotImplementedException>();
   }

   private sealed class UnknownPage : ContentPage;
}
