namespace Relate.AppLogic.Utils;

public class PseudoRandomBackgroundColor
{
   // Kept in sync by convention with the AvatarPastel01..12 entries in Resources/Styles/Colors.xaml.
   private static readonly List<Color> AvailableColors = [
      Color.FromArgb("#FBD8DC"), // AvatarPastel01 - soft rose
      Color.FromArgb("#FCE3C9"), // AvatarPastel02 - soft apricot
      Color.FromArgb("#FBEFC0"), // AvatarPastel03 - soft butter
      Color.FromArgb("#DFEFC7"), // AvatarPastel04 - soft lime
      Color.FromArgb("#C9EAD3"), // AvatarPastel05 - soft mint
      Color.FromArgb("#C3E9E4"), // AvatarPastel06 - soft seafoam
      Color.FromArgb("#C6E4F5"), // AvatarPastel07 - soft sky
      Color.FromArgb("#CFD6F6"), // AvatarPastel08 - soft periwinkle
      Color.FromArgb("#DCCBF2"), // AvatarPastel09 - soft lilac
      Color.FromArgb("#F0C9E8"), // AvatarPastel10 - soft orchid
      Color.FromArgb("#F5D0DE"), // AvatarPastel11 - soft blush
      Color.FromArgb("#D8D8D8"), // AvatarPastel12 - soft neutral gray
      ];

   // combine multiple seed parts (e.g. display name + contact id) so the color
   // stays the same for a given contact across app restarts and renames don't
   // collide two different contacts onto the same color
   public static Microsoft.Maui.Graphics.Color Get(params string?[]? seedParts)
   {
      var input = string.Join("|", (seedParts ?? []).Where(part => !string.IsNullOrWhiteSpace(part)));

      if (string.IsNullOrWhiteSpace(input))
      {
         return Colors.DarkRed;
      }
      if (AvailableColors.Count == 0)
      {
         return Colors.Blue;
      }

      var index = (int) (StableHash(input) % (uint) AvailableColors.Count);
      return AvailableColors[index];
   }

   // string.GetHashCode() is randomized per process in .NET (by design, for security),
   // so it must not be used here - it would give a different color each app run.
   // FNV-1a gives the same result every time for the same input.
   private static uint StableHash(string input)
   {
      unchecked
      {
         const uint fnvOffsetBasis = 2166136261;
         const uint fnvPrime = 16777619;

         var hash = fnvOffsetBasis;
         foreach (var c in input)
         {
            hash ^= c;
            hash *= fnvPrime;
         }

         return hash;
      }
   }
}