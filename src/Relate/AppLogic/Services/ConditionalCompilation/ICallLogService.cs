using OneOf;
using Relate.AppLogic.CallProcessing;
using Relate.AppLogic.Utils.OneOfActions;

namespace Relate.AppLogic.Services.ConditionalCompilation;

public interface ICallLogService
{
   OneOf<List<CallLogEntry>, ActionError> RetrieveCallLog();
}
