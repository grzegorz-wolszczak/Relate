using Relate.AppLogic;
using Relate.AppLogic.CallProcessing;

namespace UnitTests.AppLogic;

public class CallDisplayHelperLastContactCardTests
{
   private static readonly DateTimeOffset Now = At.Utc(2026, 8, 27, 12);

   [Fact]
   public void With_no_last_contact_the_card_says_never()
   {
      // Act
      var text = CallDisplayHelper.GetLastContactCardDisplay(null, null, null, Now, TimeZoneInfo.Utc);

      // Assert
      text.Should().Be("Contact: never");
   }

   [Fact]
   public void A_phone_call_shows_the_type_date_and_duration()
   {
      // Arrange
      var lastContact = At.Utc(2026, 8, 20, 9);

      // Act
      var text = CallDisplayHelper.GetLastContactCardDisplay(
         lastContact, ContactType.Call, TimeSpan.FromMinutes(50) + TimeSpan.FromSeconds(6), Now, TimeZoneInfo.Utc);

      // Assert
      text.Should().Contain("Contact: 7d ago <phone call>");
      text.Should().Contain("at: 2026-08-20");
      text.Should().Contain("(took: 50m6s)");
   }

   [Fact]
   public void A_meeting_shows_the_type_and_date_but_no_duration_line()
   {
      // Arrange
      var lastContact = At.Utc(2026, 8, 20);

      // Act
      var text = CallDisplayHelper.GetLastContactCardDisplay(lastContact, ContactType.Meeting, null, Now, TimeZoneInfo.Utc);

      // Assert
      text.Should().Contain("Contact: 7d ago <meeting>");
      text.Should().Contain("at: 2026-08-20");
      text.Should().NotContain("took");
   }

   [Fact]
   public void A_video_call_shows_the_type_and_date_but_no_duration_line()
   {
      // Arrange
      var lastContact = At.Utc(2026, 8, 20);

      // Act
      var text = CallDisplayHelper.GetLastContactCardDisplay(lastContact, ContactType.VideoCall, null, Now, TimeZoneInfo.Utc);

      // Assert
      text.Should().Contain("Contact: 7d ago <video call>");
      text.Should().NotContain("took");
   }

   [Fact]
   public void The_date_is_rendered_in_the_supplied_time_zone()
   {
      // Arrange
      var lastContact = At.Utc(2026, 8, 20, 23);
      var plusTwo = TimeZoneInfo.CreateCustomTimeZone("t", TimeSpan.FromHours(2), "t", "t");

      // Act
      var text = CallDisplayHelper.GetLastContactCardDisplay(lastContact, ContactType.Meeting, null, Now, plusTwo);

      // Assert
      text.Should().Contain("at: 2026-08-21"); // 23:00 UTC + 2h rolls into the next calendar day
   }
}
