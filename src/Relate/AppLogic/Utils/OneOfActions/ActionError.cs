namespace Relate.AppLogic.Utils.OneOfActions;

public class ActionError
{
    public string Message { get; }

    public ActionError(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Message = message;
    }
};