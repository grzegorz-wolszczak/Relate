using Relate.AppLogic.Services.ConditionalCompilation;

namespace UnitTests.Storage;

public class BackupDirectoryResolverTests
{
   [Fact]
   public void Returns_the_first_candidate_that_probes_successfully()
   {
      var result = BackupDirectoryResolver.Resolve(
         ["/documents", "/downloads"],
         dir => dir == "/documents");

      result.Should().Be("/documents");
   }

   [Fact]
   public void Skips_a_candidate_whose_probe_returns_false()
   {
      var result = BackupDirectoryResolver.Resolve(
         ["/documents", "/downloads"],
         dir => dir == "/downloads");

      result.Should().Be("/downloads");
   }

   [Fact]
   public void Skips_a_candidate_whose_probe_throws()
   {
      var result = BackupDirectoryResolver.Resolve(
         ["/documents", "/downloads"],
         dir => dir == "/documents" ? throw new UnauthorizedAccessException() : true);

      result.Should().Be("/downloads");
   }

   [Fact]
   public void Skips_null_and_blank_candidates()
   {
      var probed = new List<string>();

      var result = BackupDirectoryResolver.Resolve(
         [null, "  ", "/downloads"],
         dir => { probed.Add(dir); return true; });

      result.Should().Be("/downloads");
      probed.Should().Equal("/downloads");
   }

   [Fact]
   public void Returns_null_when_no_candidate_is_writable()
   {
      var result = BackupDirectoryResolver.Resolve(
         ["/documents", "/downloads"],
         _ => false);

      result.Should().BeNull();
   }
}
