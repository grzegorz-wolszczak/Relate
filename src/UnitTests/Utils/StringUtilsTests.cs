using Relate.AppLogic.Utils;

namespace UnitTests.Utils;

public class StringUtilsTests
{
   [Theory]
   [InlineData("John Smith", "JS")]
   [InlineData("smith, john", "SJ")]
   [InlineData("Anna", "A")]
   [InlineData("  Bob  ", "B")]
   [InlineData("mary jane watson", "MJW")]
   [InlineData("a,b , c", "ABC")]
   public void GetInitials_takes_the_uppercased_first_letter_of_each_token(string input, string expected)
   {
      // Act
      var result = StringUtils.GetInitials(input);

      // Assert
      result.Should().Be(expected);
   }

   [Fact]
   public void GetInitials_null_returns_placeholder()
   {
      // Act
      var result = StringUtils.GetInitials(null);

      // Assert
      result.Should().Be("<NULL>");
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData(" , , ")]
   public void GetInitials_blank_input_returns_empty(string input)
   {
      // Act
      var result = StringUtils.GetInitials(input);

      // Assert
      result.Should().BeEmpty();
   }
}
