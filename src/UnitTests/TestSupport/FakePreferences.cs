using Microsoft.Maui.Storage;

namespace UnitTests.TestSupport;

/// <summary>
/// In-memory <see cref="IPreferences"/> for tests. Dictionary-backed, no platform.
/// </summary>
public sealed class FakePreferences : IPreferences
{
   private readonly Dictionary<string, object?> _store = new();

   private static string Key(string key, string? sharedName) => $"{sharedName ?? string.Empty}::{key}";

   public IReadOnlyDictionary<string, object?> Snapshot => _store;

   public bool ContainsKey(string key, string? sharedName) => _store.ContainsKey(Key(key, sharedName));

   public void Remove(string key, string? sharedName) => _store.Remove(Key(key, sharedName));

   public void Clear(string? sharedName)
   {
      var prefix = $"{sharedName ?? string.Empty}::";
      foreach (var k in _store.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList())
      {
         _store.Remove(k);
      }
   }

   public void Set<T>(string key, T value, string? sharedName) => _store[Key(key, sharedName)] = value;

   public T Get<T>(string key, T defaultValue, string? sharedName)
      => _store.TryGetValue(Key(key, sharedName), out var value) && value is T typed ? typed : defaultValue;
}
