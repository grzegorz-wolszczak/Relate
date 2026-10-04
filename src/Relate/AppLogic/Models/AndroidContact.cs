namespace Relate.AppLogic.Models;

public class AndroidContact
{
   public required string DisplayName { get; init; }
   public required string Id { get; init; }
   public required AndroidPhoneNumber[] Phones { get; init; }
}