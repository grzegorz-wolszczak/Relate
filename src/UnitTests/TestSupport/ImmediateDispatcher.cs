using Microsoft.Maui.Dispatching;

namespace UnitTests.TestSupport;

/// <summary>
/// <see cref="IDispatcher"/> that runs every dispatched action synchronously on the calling thread.
/// </summary>
public sealed class ImmediateDispatcher : IDispatcher
{
   public bool IsDispatchRequired => false;

   public bool Dispatch(Action action)
   {
      action();
      return true;
   }

   public bool DispatchDelayed(TimeSpan delay, Action action)
   {
      action();
      return true;
   }

   public IDispatcherTimer CreateTimer() => new StubTimer();

   private sealed class StubTimer : IDispatcherTimer
   {
      public TimeSpan Interval { get; set; }
      public bool IsRepeating { get; set; }
      public bool IsRunning { get; private set; }
      public event EventHandler? Tick;

      public void Start() => IsRunning = true;

      public void Stop() => IsRunning = false;

      public void FireTick() => Tick?.Invoke(this, EventArgs.Empty);
   }
}
