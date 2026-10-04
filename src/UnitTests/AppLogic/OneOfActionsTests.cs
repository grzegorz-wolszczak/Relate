using Relate.AppLogic.Utils.OneOfActions;

namespace UnitTests.AppLogic;

public class OneOfActionsTests
{
   [Fact]
   public void ActionError_keeps_its_message()
   {
      // Act
      var error = new ActionError("boom");

      // Assert
      error.Message.Should().Be("boom");
   }

   [Fact]
   public void ActionError_rejects_a_null_message()
   {
      // Act
      var act = () => new ActionError(null!);

      // Assert
      act.Should().Throw<ArgumentNullException>();
   }

   [Fact]
   public void ActionMissingPermission_keeps_its_message_and_rejects_null()
   {
      // Act
      var permission = new ActionMissingPermission("no contacts");
      var act = () => new ActionMissingPermission(null!);

      // Assert
      permission.Message.Should().Be("no contacts");
      act.Should().Throw<ArgumentNullException>();
   }

   [Fact]
   public void ActionCancelled_is_a_stable_singleton()
   {
      // Act
      var instance = ActionCancelled.Instance;

      // Assert
      instance.Should().BeSameAs(ActionCancelled.Instance);
   }
}
