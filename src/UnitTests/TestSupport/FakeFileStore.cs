using Relate.Storage;

namespace UnitTests.TestSupport;

/// <summary>In-memory <see cref="IFileStore"/> for tests.</summary>
public sealed class FakeFileStore : IFileStore
{
   private readonly Dictionary<string, string> _files = new();

   public bool FailWrites { get; set; }
   public bool FailReads { get; set; }

   public bool Exists(string path) => _files.ContainsKey(path);

   public string? Read(string path) => _files.GetValueOrDefault(path);

   public void Seed(string path, string contents) => _files[path] = contents;

   public Task WriteAllTextAsync(string path, string contents)
   {
      if (FailWrites)
      {
         throw new IOException("write failed");
      }

      _files[path] = contents;
      return Task.CompletedTask;
   }

   public Task<string> ReadAllTextAsync(string path)
   {
      if (FailReads)
      {
         throw new IOException("read failed");
      }

      return _files.TryGetValue(path, out var contents)
         ? Task.FromResult(contents)
         : throw new FileNotFoundException(path);
   }
}
