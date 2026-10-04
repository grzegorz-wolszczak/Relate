namespace Relate.AppLogic.Services.ConditionalCompilation;

/// <summary>
/// Picks the first backup directory that can actually be written to. Kept free of any
/// platform API so it can be unit-tested with a fake probe.
/// </summary>
public static class BackupDirectoryResolver
{
   /// <summary>
   /// Walks <paramref name="candidates"/> in order and returns the first one for which
   /// <paramref name="prepareAndProbe"/> succeeds (creates the directory and confirms a
   /// real write). Null/blank candidates are skipped; a probe that returns false or
   /// throws moves on to the next candidate. Returns null when none work.
   /// </summary>
   public static string? Resolve(IEnumerable<string?> candidates, Func<string, bool> prepareAndProbe)
   {
      foreach (var candidate in candidates)
      {
         if (string.IsNullOrWhiteSpace(candidate))
         {
            continue;
         }

         try
         {
            if (prepareAndProbe(candidate))
            {
               return candidate;
            }
         }
         catch
         {
            // not writable - try the next candidate
         }
      }

      return null;
   }
}
