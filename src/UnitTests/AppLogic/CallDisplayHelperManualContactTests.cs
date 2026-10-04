using Relate.AppLogic;

namespace UnitTests.AppLogic;

public class CallDisplayHelperManualContactTests
{
   [Fact]
   public void GetManualContactDisplay_with_no_date_shows_not_available()
   {
      // Arrange & Act
      var text = CallDisplayHelper.GetManualContactDisplay("meeting", null);

      // Assert
      text.Should().Be("Last meeting: <N/A>");
   }

   [Fact]
   public void GetManualContactDisplay_with_a_date_shows_it_formatted()
   {
      // Arrange
      var date = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.FromHours(2));

      // Act
      var text = CallDisplayHelper.GetManualContactDisplay("video call", date);

      // Assert
      text.Should().Be("Last video call: 2026-05-01");
   }
}
