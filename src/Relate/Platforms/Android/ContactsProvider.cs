using Android.Content;
using Android.Database;
using Android.Provider;
using Relate.AppLogic.Models;

namespace Relate;

public class ContactsProvider
{
   private static readonly List<AndroidContact> Empty = new();

   public static List<AndroidContact> LoadContacts()
   {
      var result = new List<AndroidContact>();
      var uri = ContactsContract.Contacts.ContentUri;
      var ctx = Android.App.Application.Context;
      var projection = new[]
      {
         ContactsContract.Contacts.InterfaceConsts.Id,
         ContactsContract.Contacts.InterfaceConsts.DisplayName,
         //ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri
      };
      var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} ASC";

      if (uri is null)
      {
         return Empty;
      }

      using ICursor? cursor = ctx.ContentResolver?.Query(
         uri,
         projection,
         selection: null,
         selectionArgs: null,
         sortOrder);

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
         var contactId = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.Id);

         if (contactId is null)
         {
            continue;
         }

         //var photoThumbnailUri = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri);
         var displayName = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.DisplayName);
         var numbers = GetNumbers(ctx, contactId);

         result.Add(new()
         {
            DisplayName = displayName ?? $"Contact:{contactId}",
            Id = contactId,
            Phones = numbers
         });



      } while (cursor.MoveToNext());

      cursor.Close();
      return result;
   }


   static AndroidPhoneNumber[] GetNumbers(Context ctx, string contactId)
   {
      using var cursor = ctx.ContentResolver?.Query(
         ContactsContract.CommonDataKinds.Phone.ContentUri,
         null,
         ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = ?",
         new[] {contactId},
         null
      );

      if (cursor is null)
      {
         return Array.Empty<AndroidPhoneNumber>();
      }

      var numbers = ReadPhoneCursorItems(cursor).ToArray();

      cursor.Close();
      return numbers;
   }

   static IEnumerable<AndroidPhoneNumber> ReadPhoneCursorItems(ICursor cursor)
   {
      var numberIdx = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
      var typeIdx = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type);
      var labelIdx = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label);

      while (cursor.MoveToNext())
      {
         var number = cursor.GetString(numberIdx);
         if (string.IsNullOrEmpty(number))
         {
            continue;
         }

         var type = typeIdx >= 0 ? (PhoneDataKind)cursor.GetInt(typeIdx) : PhoneDataKind.Other;
         var customLabel = labelIdx >= 0 ? cursor.GetString(labelIdx) : null;
         yield return new AndroidPhoneNumber(number, GetHumanLabel(type, customLabel));
      }
   }

   static string GetHumanLabel(PhoneDataKind type, string? customLabel)
   {
      return ContactsContract.CommonDataKinds.Phone
         .GetTypeLabel(Android.App.Application.Context.Resources, type, customLabel)
         ?.ToString() ?? "Other";
   }

   static string? GetString(ICursor cursor, string key)
   {
      return cursor.GetString(cursor.GetColumnIndex(key));
   }


   public static AndroidContact? GetContactById(string? id)
   {
      if (id is null) return null;
      var tempResults = new List<AndroidContact>();
      var uri = ContactsContract.Contacts.ContentUri;
      var ctx = Android.App.Application.Context;
      var projection = new string[]
      {
         ContactsContract.Contacts.InterfaceConsts.Id,
         ContactsContract.Contacts.InterfaceConsts.DisplayName,
         //ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri
      };


      using ICursor? cursor = ctx.ContentResolver.Query(
         uri,
         projection,
         selection: ContactsContract.Contacts.InterfaceConsts.Id + " = ?",
         new[] {id},
         sortOrder: null);

      if (cursor is null)
      {
         return null;
      }

      if (!cursor.MoveToFirst())
      {
         return null;
      }

      do
      {
         string contactId = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.Id);
         //var photoThumbnailUri = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri);
         var displayName = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.DisplayName);
         var numbers = GetNumbers(ctx, contactId);

         tempResults.Add(new()
         {
            DisplayName = displayName,
            Id = contactId,
            Phones = numbers
         });

      } while (cursor.MoveToNext());

      cursor.Close();

      if (tempResults.Count == 0)
      {
         return null;
      }

      if (tempResults.Count > 1)
      {
         // bad query
         return null;
      }
      return tempResults[0];
   }
}