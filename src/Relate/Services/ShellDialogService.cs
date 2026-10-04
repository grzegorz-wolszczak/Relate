namespace Relate.Services;

public sealed class ShellDialogService : IDialogService
{
   public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
      => Shell.Current.DisplayAlert(title, message, accept, cancel);

   public Task AlertAsync(string title, string message, string cancel)
      => Shell.Current.DisplayAlert(title, message, cancel);

   public async Task<string?> PickOptionAsync(string title, string cancel, params string[] options)
   {
      var result = await Shell.Current.DisplayActionSheet(title, cancel, null, options);
      return result == cancel ? null : result;
   }
}
