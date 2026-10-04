using System.Reflection;

namespace Relate;

public static class BuildInfo
{
   // "0.0.1-<build>" on Android - set from VersionPrefix/AppBuildNumber in Relate.csproj
   public static string Version { get; } =
      typeof(BuildInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
      ?? "unknown";
}
