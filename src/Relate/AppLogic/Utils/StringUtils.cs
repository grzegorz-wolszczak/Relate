using System.Globalization;
using System.Text;

namespace Relate.AppLogic.Utils;

public static class StringUtils
{
   public static string GetInitials(string? name)
   {
      if (name == null)
      {
         return "<NULL>";
      }
      var nameSplit = name.Split([",", " "],
         StringSplitOptions.RemoveEmptyEntries);

      var builder = new StringBuilder();

      foreach (string item in nameSplit)
      {
         builder.Append(item[..1].ToUpper(CultureInfo.InvariantCulture));
      }

      return builder.ToString();
   }
}