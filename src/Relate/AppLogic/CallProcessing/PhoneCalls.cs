namespace Relate.AppLogic.CallProcessing;

public class PhoneCalls
{
   private readonly List<PhoneCall> _calls;

   public PhoneCalls(List<PhoneCall> calls)
   {
      _calls = calls;
   }

   public List<PhoneCall> GetLongCalls(TimeSpan longCallDuration)
   {
      var foundLongCalls = _calls
         .Where(c => c.CallDuration >= longCallDuration)
         .OrderByDescending(c => c.CallDateTime).ToList();

      return foundLongCalls;
   }
}