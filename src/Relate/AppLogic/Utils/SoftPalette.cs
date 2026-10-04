namespace Relate.AppLogic.Utils;

// C# mirror of the "Soft Pastel / Neumorphic" design tokens defined in
// Resources/Styles/Colors.xaml. The list/details pages, the settings tab and
// the contact card template are built entirely in C#
// (CommunityToolkit.Maui.Markup, no XAML), so they can't reach StaticResource
// lookups - these constants are the single place those pages pull the palette
// from instead of repeating hex literals. Keep in sync with Colors.xaml if the
// palette changes. Light theme only, per requirements.
public static class SoftPalette
{
   public static readonly Color PageBackground = Color.FromArgb("#F3F0FB");
   public static readonly Color CardBackground = Color.FromArgb("#FFFFFF");
   public static readonly Color CardStroke = Color.FromArgb("#ECE6F9");
   public static readonly Color ShadowBase = Color.FromArgb("#3A2E63");

   // Filled pastel panel used by the Settings tab sections.
   public static readonly Color SectionBackground = Color.FromArgb("#E7E1FA");

   public static readonly Color TextPrimary = Color.FromArgb("#2E2350");
   public static readonly Color TextSecondary = Color.FromArgb("#847DA0");

   public static readonly Color AccentPrimary = Color.FromArgb("#6C4FE0");
   public static readonly Color AccentTeal = Color.FromArgb("#3FB8AE");
   public static readonly Color AccentDanger = Color.FromArgb("#F0665F");

   public static Shadow SoftShadow(float radius = 16, float opacity = 0.18f, double offsetY = 6) =>
      new()
      {
         Brush = new SolidColorBrush(ShadowBase),
         Radius = radius,
         Opacity = opacity,
         Offset = new Point(0, offsetY),
      };
}
