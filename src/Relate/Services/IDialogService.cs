namespace Relate.Services;

public interface IDialogService
{
   Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
   Task AlertAsync(string title, string message, string cancel);
   Task<string?> PickOptionAsync(string title, string cancel, params string[] options);
}
