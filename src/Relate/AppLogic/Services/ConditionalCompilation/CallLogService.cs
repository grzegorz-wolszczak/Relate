using OneOf;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public class CallLogService : ICallLogService
{

    public OneOf<List<CallLogEntry>, ActionError> RetrieveCallLog()
    {
#if WINDOWS
        return new ActionError("Call log is not supported on Windows.");
#elif ANDROID
    return CallLogProvider.GetCallLogs();
#else
    return new ActionError("Unsupported platform.");
#endif
    }
}