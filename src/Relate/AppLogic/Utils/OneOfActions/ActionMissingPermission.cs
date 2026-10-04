namespace Relate.AppLogic.Utils.OneOfActions;

public class ActionMissingPermission
{
   public string Message { get; }

   public ActionMissingPermission(string message)
   {
      ArgumentNullException.ThrowIfNull(message);
      Message = message;
   }
};