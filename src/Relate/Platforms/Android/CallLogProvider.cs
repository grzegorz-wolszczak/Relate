using Android.Database;
using Android.Provider;
using Relate.AppLogic.CallProcessing;


namespace Relate;

public static class CallLogProvider
{
   private static readonly List<CallLogEntry> Empty = new();
    public static List<CallLogEntry> GetCallLogs()
    {
        var result = new List<CallLogEntry>();

        var uri = CallLog.Calls.ContentUri;
        string[] projection = {
            CallLog.Calls.Number,
            //CallLog.Calls.Type,
            CallLog.Calls.Date,
            CallLog.Calls.Duration,
            CallLog.Calls.CachedName,
        };

        if (uri is null)
        {
           return Empty;
        }

        //var sortOrder = CallLog.Calls.Date + " DESC";
        var sortOrder = CallLog.Calls.DefaultSortOrder;

        using ICursor? cursor = Android.App.Application.Context.ContentResolver?.Query(
           uri,
           projection,
           selection:null,
           selectionArgs: null,
           sortOrder: sortOrder);

        if (cursor is null)
        {
           return Empty;
        }

        if (!cursor.MoveToFirst())
        {
           return result;
        }

        do
        {
           var number = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Number));

           var ticks = cursor.GetLong(cursor.GetColumnIndex(CallLog.Calls.Date));
           var date = DateTimeOffset.FromUnixTimeMilliseconds(ticks);
           var duration = cursor.GetLong(cursor.GetColumnIndex(CallLog.Calls.Duration));
           var name = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.CachedName));

           result.Add(new()
           {
              Number = number, Date = date, DurationSeconds = duration, PersonWhoCalledName = name,
           });
        } while (cursor.MoveToNext());

        cursor.Close();

        return result;
    }

}