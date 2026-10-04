using Relate.AppLogic.Models;
using Relate.AppLogic.Services.ConditionalCompilation;

namespace UnitTests.AppLogic;

public class MySystemContactTests
{
   [Fact]
   public void Maps_id_and_display_name_from_an_android_contact()
   {
      // Arrange
      var android = new AndroidContact { Id = "abc", DisplayName = "Grace Hopper", Phones = [new("+48111222333", "")] };

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Id.Should().Be("abc");
      contact.DisplayName.Should().Be("Grace Hopper");
   }

   [Fact]
   public void Maps_every_phone_number()
   {
      // Arrange
      var android = new AndroidContact { Id = "1", DisplayName = "n", Phones = [new("111", ""), new("222", ""), new("333", "")] };

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Phones.Select(p => p.PhoneNumber).Should().Equal("111", "222", "333");
   }

   [Fact]
   public void Handles_a_contact_with_no_phones()
   {
      // Arrange
      var android = new AndroidContact { Id = "1", DisplayName = "n", Phones = [] };

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Phones.Should().BeEmpty();
   }

   [Fact]
   public void Deduplicates_the_exact_same_number_reported_twice()
   {
      // Arrange - e.g. the same contact synced from two accounts, each with an identical entry
      var android = new AndroidContact
         {Id = "1", DisplayName = "n", Phones = [new("503181146", "Mobile"), new("503181146", "Mobile")]};

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Phones.Should().ContainSingle(p => p.PhoneNumber == "503181146");
   }

   [Fact]
   public void Deduplicates_the_same_number_formatted_differently()
   {
      // Arrange - spaces and a "+48" prefix are cosmetic; the underlying number is identical
      var android = new AndroidContact
      {
         Id = "1", DisplayName = "n",
         Phones = [new("503181146", "Mobile"), new("503 181 146", "Mobile"), new("+48503181146", "Mobile")]
      };

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Phones.Should().ContainSingle();
   }

   [Fact]
   public void Keeps_genuinely_different_numbers()
   {
      // Arrange
      var android = new AndroidContact
         {Id = "1", DisplayName = "n", Phones = [new("111", "Mobile"), new("222", "Home")]};

      // Act
      var contact = new MySystemContact(android);

      // Assert
      contact.Phones.Select(p => p.PhoneNumber).Should().Equal("111", "222");
   }
}
