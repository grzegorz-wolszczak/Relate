using Relate.AppLogic;

namespace UnitTests.AppLogic;

public class CallDisplayHelperNextCallTests
{
   [Fact]
   public void No_last_contact_reads_as_today()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetNextContactInDaysDisplayDetails(99, hasLastContact: false);

      // Assert
      text.Should().Be("next contact: today!");
   }

   [Fact]
   public void Due_today_reads_as_today()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetNextContactInDaysDisplayDetails(0, hasLastContact: true);

      // Assert
      text.Should().Be("next contact: today!");
   }

   [Fact]
   public void One_day_reads_as_tomorrow()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetNextContactInDaysDisplayDetails(1, hasLastContact: true);

      // Assert
      text.Should().Be("next contact: tomorrow");
   }

   [Fact]
   public void Several_days_reads_as_in_n_days()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetNextContactInDaysDisplayDetails(5, hasLastContact: true);

      // Assert
      text.Should().Be("next contact: in 5d");
   }

   [Fact]
   public void Overdue_reads_as_missed()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetNextContactInDaysDisplayDetails(-3, hasLastContact: true);

      // Assert
      text.Should().Be("next contact: today! [missed: 3d ago]");
   }
}
