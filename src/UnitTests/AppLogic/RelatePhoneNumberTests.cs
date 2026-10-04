using Relate.AppLogic;

namespace UnitTests.AppLogic;

public class RelatePhoneNumberTests
{
   [Theory]
   [InlineData("123 456 789", "123456789")]
   [InlineData("123-456-789", "123456789")]
   [InlineData("(123) 456-789", "123456789")]
   [InlineData("call me: 123abc456", "123456")]
   public void Strips_everything_that_is_not_a_digit_or_plus(string input, string expected)
   {
      // Act
      var number = new RelatePhoneNumber(input);

      // Assert
      number.Number.Should().Be(expected);
   }

   [Theory]
   [InlineData("+48 123 456 789", "123456789")]
   [InlineData("0048123456789", "123456789")]
   [InlineData("+48123456789", "123456789")]
   public void Removes_the_polish_country_prefix(string input, string expected)
   {
      // Act
      var number = new RelatePhoneNumber(input);

      // Assert
      number.Number.Should().Be(expected);
   }

   [Theory]
   [InlineData("+12025550173", "+12025550173")]
   [InlineData("48123456789", "48123456789")]
   [InlineData("123456789", "123456789")]
   public void Leaves_other_numbers_untouched(string input, string expected)
   {
      // Act
      var number = new RelatePhoneNumber(input);

      // Assert
      number.Number.Should().Be(expected);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("---")]
   public void Blank_or_null_normalizes_to_empty(string? input)
   {
      // Act
      var number = new RelatePhoneNumber(input);

      // Assert
      number.Number.Should().BeEmpty();
   }

   [Fact]
   public void Two_differently_formatted_equal_numbers_compare_equal()
   {
      // Arrange
      var a = new RelatePhoneNumber("+48 123 456 789");
      var b = new RelatePhoneNumber("123-456-789");

      // Act
      var areEqual = a.Equals(b);

      // Assert
      areEqual.Should().BeTrue();
      a.GetHashCode().Should().Be(b.GetHashCode());
   }

   [Fact]
   public void Equality_makes_list_contains_work_by_value()
   {
      // Arrange
      var contactNumbers = new List<RelatePhoneNumber> { new("123456789"), new("987654321") };

      // Act
      var matches = contactNumbers.Contains(new RelatePhoneNumber("0048 123 456 789"));
      var noMatch = contactNumbers.Contains(new RelatePhoneNumber("555"));

      // Assert
      matches.Should().BeTrue();
      noMatch.Should().BeFalse();
   }
}
