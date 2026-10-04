using System.ComponentModel;
using Microsoft.Extensions.Logging.Abstractions;
using Relate.ViewModels;

namespace UnitTests.ViewModels;

public class ProblemReporterVmTests
{
   private readonly ProblemReporterVm _sut =
      new(NullLogger<ProblemReporterVm>.Instance, new ImmediateDispatcher());

   [Fact]
   public void LogError_appends_a_tagged_line_to_the_log_content()
   {
      // Act
      _sut.LogError("disk full");

      // Assert
      _sut.LogContent.Should().Contain("[err]: disk full");
   }

   [Fact]
   public void LogDebug_appends_a_tagged_line_to_the_log_content()
   {
      // Act
      _sut.LogDebug("loaded 12 contacts");

      // Assert
      _sut.LogContent.Should().Contain("[dbg]: loaded 12 contacts");
   }

   [Fact]
   public void Successive_messages_accumulate()
   {
      // Act
      _sut.LogDebug("one");
      _sut.LogError("two");
      _sut.LogDebug("three");

      // Assert
      _sut.LogContent.Should().Contain("one").And.Contain("[err]: two").And.Contain("three");
      _sut.LogContent.IndexOf("one", StringComparison.Ordinal)
         .Should().BeLessThan(_sut.LogContent.IndexOf("three", StringComparison.Ordinal));
   }

   [Fact]
   public void Updating_the_log_raises_property_changed_for_LogContent()
   {
      // Arrange
      var raised = new List<string?>();
      ((INotifyPropertyChanged)_sut).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

      // Act
      _sut.LogError("boom");

      // Assert
      raised.Should().Contain(nameof(ProblemReporterVm.LogContent));
   }
}
