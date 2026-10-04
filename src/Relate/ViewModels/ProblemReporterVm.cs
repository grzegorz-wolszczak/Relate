using System.Text;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Relate.ViewModels;

public partial class ProblemReporterVm : BaseVm
{
   private readonly ILogger<ProblemReporterVm> _loger;
   private readonly IDispatcher _dispatcher;


   [ObservableProperty]
   private string _logContent;

   private StringBuilder _builder = new();

   public ProblemReporterVm(ILogger<ProblemReporterVm> loger, IDispatcher dispatcher)
   {
      _loger = loger;
      _dispatcher = dispatcher;
   }

   public void LogError(string errorMessage)
   {
      _dispatcher.Dispatch(() => Append("err", errorMessage));
   }

   public void LogDebug(string message)
   {
      // LogContent is a bound property - must be mutated on the UI thread, like LogError
      _dispatcher.Dispatch(() => Append("dbg", message));
   }

   public void ShowErrorToUserAndLog(string errorMessage)
   {
      Toast.Make(errorMessage).Show();
      LogError(errorMessage);
   }

   private void Append(string tag, string message)
   {
      var formatted = $"------{Environment.NewLine}" +
                      $"[{tag}]: {message}{Environment.NewLine}";
      _builder.Append(formatted);
      LogContent = _builder.ToString();
   }
}