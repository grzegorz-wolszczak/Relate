namespace Relate.Services;

public interface INavigationService
{
   Task GoToAsync(string route);
   Task GoBackAsync();
}
