namespace Relate.AppLogic;

public sealed record RelatePhoneNumber
{
   private readonly string _number;

   public string Number
   {
      get => _number;
   }

   public RelatePhoneNumber(string? value)
   {
      // null/empty (e.g. from malformed stored JSON) must not crash contact loading -
      // an empty number simply matches no call.
      _number = NormalizePhoneNumber(value ?? string.Empty);
   }

   private static string NormalizePhoneNumber(string value)
   {
      // removal all characters except from '+'
      value = new(value.Where(c => char.IsDigit(c) || c == '+').ToArray());

      // cut out +48
      if (value.StartsWith("+48"))
      {
         return value.Substring(3);
      }

      // or cut out '0048'
      if (value.StartsWith("0048"))
      {
         return value.Substring(4);
      }

      return value;
   }
}