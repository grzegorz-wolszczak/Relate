using System.Text.Json;
using CSharpFunctionalExtensions;
using Relate.AppLogic.Utils.OneOfActions;
using Relate.ViewModels;

namespace Relate.Storage;
using OneOf;

public class StorageService
{
   private readonly IPreferences _preferences;
   private readonly IFileStore _fileStore;
   private const string SerializedListKey = nameof(SerializedListKey);
   private const string DefaultSettingsKey = nameof(DefaultSettingsKey);

   public StorageService(IPreferences preferences, IFileStore fileStore)
   {
      _preferences = preferences;
      _fileStore = fileStore;
   }

   public List<RelateContactDto> ReadContactsFromStorage()
   {
      return ReadFromPreferences<List<RelateContactDto>>(SerializedListKey, []);
   }

   private T ReadFromPreferences<T>(string key, T defaultValue)
   {
      if (!_preferences.ContainsKey(key))
      {
         return defaultValue;
      }

      var serializedList = _preferences.Get(key, string.Empty);
      if (string.IsNullOrWhiteSpace(serializedList))
      {
         return defaultValue;
      }

      return JsonSerializer
         .Deserialize<T>(serializedList) ?? defaultValue;
   }

   public void SaveContacts(List<RelateContactDto> contactsDto)
   {
      // make sure we are not saving duplicates
      var finalList = GetUniqueContacts(contactsDto);

      SaveToPreferences(SerializedListKey, finalList);
   }

   private static List<RelateContactDto> GetUniqueContacts(List<RelateContactDto> contactsDto)
   {
      var finalList = new List<RelateContactDto>();
      foreach (var relateContactDto in contactsDto)
      {
         if (!(finalList.Any(x => x.Id == relateContactDto.Id)))
         {
            finalList.Add(relateContactDto);
         }
      }

      return finalList;
   }

   public void SaveDefaultSettings(DefaultSettingsDto settings)
   {
      SaveToPreferences(DefaultSettingsKey, settings);
   }

   private void SaveToPreferences<T>(string key, T value)
   {
      var asString = JsonSerializer.Serialize(value);
      _preferences.Set(key, asString);
   }

   public DefaultSettingsDto ReadDefaultSettings()
   {
      return ReadFromPreferences<DefaultSettingsDto>(DefaultSettingsKey, new());
   }

   public async Task<Maybe<ActionError>> SaveBackup(ContactsList contactList,
      string backupFilePath)
   {

      List<RelateContactDto> dtos = contactList.Items.ToContactsDto();

      var finalList = GetUniqueContacts(dtos);
      var asString = JsonSerializer.Serialize(finalList);
      try
      {
         await _fileStore.WriteAllTextAsync(backupFilePath, asString);
         return Maybe.None;
      }
      catch (Exception e)
      {
         return new ActionError($"Could not save backup: {e.Message}");
      }
   }

   public bool IsBackupAvailable(string filePath)
   {
      // todo: what if backup exists but is not empty?
      return _fileStore.Exists(filePath);
   }

   public async Task<OneOf<List<RelateContactDto>, ActionError>> RestoreFromBackup(string filePath)
   {
      if (!_fileStore.Exists(filePath))
      {
         return new ActionError("Backup file does not exist");
      }

      try
      {
         var text = await _fileStore.ReadAllTextAsync(filePath);
         var dtos =  JsonSerializer.Deserialize<List<RelateContactDto>>(text) ?? [];
         return dtos;
      }
      catch (Exception e)
      {
         return new ActionError($"Could not restore backup: {e.Message}");
      }
   }
}