namespace Relate.AppLogic;

public static class AppConstants
{
   private const int ThreeYearsAsDays = 1095;
   public const int MinNoContactPeriodDays = 1;
   public const int DefaultNoContactPeriodDays = 30;
   public const int MaxNoContactPeriodDays = ThreeYearsAsDays;

   private const int ThreeHoursAsMinutes = 180;

   public const int MinProperCallDurationMinutes = 1;
   public const int DefaultCallDurationMinutes = 30;
   public const int MaxProperCallDurationMinutes = ThreeHoursAsMinutes;
}