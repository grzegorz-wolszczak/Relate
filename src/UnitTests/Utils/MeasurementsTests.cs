using Relate.AppLogic.Utils;

namespace UnitTests.Utils;

public class MeasurementsTests
{
   [Fact]
   public void Action_overload_runs_the_action_and_returns_non_negative_elapsed()
   {
      // Arrange
      var ran = false;

      // Act
      var elapsed = Measurements.ExecDuration(() => { ran = true; });

      // Assert
      ran.Should().BeTrue();
      elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
   }

   [Fact]
   public void Func_overload_passes_the_result_through()
   {
      // Act
      var (elapsed, result) = Measurements.ExecDuration(() => 21 * 2);

      // Assert
      result.Should().Be(42);
      elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
   }

   [Fact]
   public void Exceptions_propagate_out_of_the_action_overload()
   {
      // Arrange
      var throwing = (Action)(() => throw new InvalidOperationException("nope"));

      // Act
      var act = () => Measurements.ExecDuration(throwing);

      // Assert
      act.Should().Throw<InvalidOperationException>().WithMessage("nope");
   }

   [Fact]
   public async Task Async_overload_awaits_the_task()
   {
      // Arrange
      var ran = false;

      // Act
      await Measurements.ExecDuration(async () =>
      {
         await Task.Yield();
         ran = true;
      });

      // Assert
      ran.Should().BeTrue();
   }
}
