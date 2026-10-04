using System.Diagnostics;

namespace Relate.AppLogic.Utils;

public class Measurements
{
   public static TimeSpan ExecDuration(Action action)
   {
      var stopWatch = Stopwatch.StartNew();
      try
      {
         action.Invoke();
      }
      finally
      {
         stopWatch.Stop();
      }
      return stopWatch.Elapsed;
   }


   public static async Task<TimeSpan> ExecDuration(Func<Task> action)
   {
      var stopWatch = Stopwatch.StartNew();
      try
      {
         await action();
      }
      finally
      {
         stopWatch.Stop();
      }
      return stopWatch.Elapsed;
   }


   public static (TimeSpan, TResult) ExecDuration<TResult>(Func<TResult> action)
   {
      var stopWatch = Stopwatch.StartNew();
      TResult result = default!;
      try
      {
         result = action();
      }
      finally
      {
         stopWatch.Stop();
      }
      return (stopWatch.Elapsed,result);
   }

}