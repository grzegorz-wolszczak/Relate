namespace Relate.Storage;

public interface IFileStore
{
   bool Exists(string path);
   Task WriteAllTextAsync(string path, string contents);
   Task<string> ReadAllTextAsync(string path);
}

public sealed class PhysicalFileStore : IFileStore
{
   public bool Exists(string path) => File.Exists(path);

   public Task WriteAllTextAsync(string path, string contents) => File.WriteAllTextAsync(path, contents);

   public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
}
