using Relate.AppLogic.Utils;

namespace UnitTests.Utils;

public class PseudoRandomBackgroundColorTests
{
   [Fact]
   public void No_usable_seed_parts_return_dark_red()
   {
      // Act
      var noArgs = PseudoRandomBackgroundColor.Get();
      var singleNull = PseudoRandomBackgroundColor.Get(null);
      var allBlank = PseudoRandomBackgroundColor.Get(null, "   ", "");

      // Assert
      noArgs.Should().Be(Colors.DarkRed);
      singleNull.Should().Be(Colors.DarkRed);
      allBlank.Should().Be(Colors.DarkRed);
   }

   [Fact]
   public void Same_seed_always_maps_to_the_same_color()
   {
      // Arrange
      var first = PseudoRandomBackgroundColor.Get("Alice Adams", "contact-42");

      // Act
      var second = PseudoRandomBackgroundColor.Get("Alice Adams", "contact-42");

      // Assert
      second.Should().Be(first);
   }

   [Fact]
   public void Blank_parts_are_ignored_when_combining_the_seed()
   {
      // Act
      var withBlank = PseudoRandomBackgroundColor.Get("Bob", null, "id-7");
      var withoutBlank = PseudoRandomBackgroundColor.Get("Bob", "id-7");

      // Assert
      withBlank.Should().Be(withoutBlank);
   }

   [Fact]
   public void Different_seeds_are_spread_across_the_palette()
   {
      // Act
      var colors = Enumerable.Range(0, 40)
         .Select(i => PseudoRandomBackgroundColor.Get($"person {i}", $"id{i}"))
         .Distinct()
         .ToList();

      // Assert
      colors.Should().HaveCountGreaterThan(5);
   }

   [Fact]
   public void Result_is_never_the_fallback_when_a_seed_is_present()
   {
      // Act
      var colors = Enumerable.Range(0, 100)
         .Select(i => PseudoRandomBackgroundColor.Get($"name{i}"))
         .ToList();

      // Assert
      colors.Should().NotContain(Colors.Blue);
   }
}
