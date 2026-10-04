namespace Relate.AppLogic.Utils;

public static class HumanReadableTime
{
   public static string DateTimeAgo(DateTimeOffset inputDate) => DateTimeAgo(inputDate, DateTimeOffset.Now);

   public static string DateTimeAgo(DateTimeOffset inputDate, DateTimeOffset now)
   {
      var difference = now - inputDate;

      // If less than a minute
      if (difference.TotalSeconds < 60)
      {
         var seconds = (int)difference.TotalSeconds;
         return $"{seconds}s ago";
      }
      // If less than an hour

      if (difference.TotalMinutes < 60)
      {
         var minutes = (int)difference.TotalMinutes;
         return $"{minutes}min ago";
      }
      // If less than a day

      if (difference.TotalHours < 24)
      {
         var hours = (int)difference.TotalHours;
         return $"{hours}h ago";
      }
      // If less than 30 days

      if (difference.TotalDays < 30)
      {
         var days = (int)difference.TotalDays;
         return $"{days}d ago";
      }
      // If less than 12 months

      if (difference.TotalDays < 365)
      {
         var months = (int)(difference.TotalDays / 30);
         var remainingDays = (int)(difference.TotalDays % 30);
         // var monthsText = months == 1 ? "month" : "months";
         var monthsText = "m";
         //var daysText = remainingDays == 1 ? "day" : "days";
         var daysText = "d";
         return $"{months}{monthsText}{remainingDays}{daysText} ago";
      }
      // If a year or more
      else
      {
         var years = (int)(difference.TotalDays / 365);
         var remainingMonths = (int)((difference.TotalDays % 365) / 30);
         var remainingDays = (int)((difference.TotalDays % 365) % 30);
         //var yearsText = years == 1 ? "year" : "years";
         var yearsText =  "y";
         //var monthsText = remainingMonths == 1 ? "month" : "months";
         var monthsText = "m";
         //var daysText = remainingDays == 1 ? "day" : "days";
         var daysText = "d";
         return $"{years}{yearsText}{remainingMonths}{monthsText}{remainingDays}{daysText} ago";
      }
   }

   public static string Duration(TimeSpan duration)
   {
      // If less than a minute
      if (duration.TotalSeconds < 60)
      {
         int seconds = (int)duration.TotalSeconds;
         return $"{seconds}s";
      }
      // If less than an hour
      else if (duration.TotalMinutes < 60)
      {
         int minutes = (int)duration.TotalMinutes;
         int seconds = (int)(duration.TotalSeconds % 60);
         return seconds > 0 ? $"{minutes}m{seconds}s" : $"{minutes}min";
      }
      // If an hour or more
      else
      {
         int hours = (int)duration.TotalHours;
         int minutes = (int)(duration.TotalMinutes % 60);
         return minutes > 0 ? $"{hours}h{minutes}m" : $"{hours}h";
      }
   }
}