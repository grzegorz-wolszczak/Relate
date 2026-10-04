using System.Collections.ObjectModel;
using Relate.ViewModels;
using static UnitTests.TestSupport.Build;

namespace UnitTests.ViewModels;

public class ContactsListTests
{
   private readonly ObservableCollection<RelateContactVm> _observable = [];
   private readonly ContactsList _sut;

   public ContactsListTests() => _sut = new ContactsList(_observable, new ImmediateDispatcher());

   private static RelateContactVm Contact(string id, string name, int nextCallInDays = 0)
   {
      var vm = new RelateContactVmBuilder().WithId(id).Named(name).Build();
      vm.NextCallInDays = nextCallInDays;
      return vm;
   }

   [Fact]
   public void AddContact_puts_it_in_both_the_list_and_the_observable()
   {
      // Arrange
      var contact = Contact("1", "Ann");

      // Act
      _sut.AddContact(contact);

      // Assert
      _sut.Items.Should().ContainSingle().Which.Should().BeSameAs(contact);
      _observable.Should().ContainSingle().Which.Should().BeSameAs(contact);
   }

   [Fact]
   public void Items_returns_a_copy()
   {
      // Arrange
      _sut.AddContact(Contact("1", "Ann"));

      // Act
      _sut.Items.Clear();

      // Assert
      _sut.Items.Should().ContainSingle();
   }

   [Fact]
   public void SortByNextCall_orders_ascending_by_next_call_in_days()
   {
      // Arrange
      _sut.Reset([Contact("1", "Late", 40), Contact("2", "Soon", 2), Contact("3", "Overdue", -5)]);

      // Act
      _sut.SortByNextCall();

      // Assert
      _observable.Select(c => c.ContactDisplayName).Should().Equal("Overdue", "Soon", "Late");
   }

   [Fact]
   public void ContactExists_matches_by_id()
   {
      // Arrange
      _sut.AddContact(Contact("abc", "Ann"));

      // Act
      var exists = _sut.ContactExists(SystemContact("abc", "Whatever"));
      var missing = _sut.ContactExists(SystemContact("xyz", "Ann"));

      // Assert
      exists.Should().BeTrue();
      missing.Should().BeFalse();
   }

   [Fact]
   public void Reset_replaces_the_whole_list()
   {
      // Arrange
      _sut.AddContact(Contact("1", "Old"));

      // Act
      _sut.Reset([Contact("2", "New A"), Contact("3", "New B")]);

      // Assert
      _sut.Items.Select(c => c.ContactId).Should().Equal("2", "3");
      _observable.Should().HaveCount(2);
   }

   [Fact]
   public void Remove_deletes_an_existing_contact_from_the_observable()
   {
      // Arrange
      _sut.Reset([Contact("1", "Ann"), Contact("2", "Bob")]);

      // Act
      var removed = _sut.Remove("1");

      // Assert
      removed.Should().BeTrue();
      _observable.Select(c => c.ContactId).Should().Equal("2");
   }

   [Fact]
   public void Remove_returns_false_for_an_unknown_id()
   {
      // Arrange
      _sut.Reset([Contact("1", "Ann")]);

      // Act
      var removed = _sut.Remove("nope");

      // Assert
      removed.Should().BeFalse();
   }

   [Fact]
   public void Remove_tolerates_duplicate_ids_and_removes_the_first()
   {
      // Arrange
      _sut.Reset([Contact("dup", "First"), Contact("dup", "Second")]);

      // Act
      var removed = _sut.Remove("dup");

      // Assert
      removed.Should().BeTrue();
      _sut.Items.Should().ContainSingle().Which.ContactDisplayName.Should().Be("Second");
   }

   [Fact]
   public void ShowFiltered_is_a_case_insensitive_trimmed_substring_match()
   {
      // Arrange
      _sut.Reset([Contact("1", "Alice Adams"), Contact("2", "Bob Brown"), Contact("3", "Charlie")]);

      // Act
      _sut.ShowFiltered("  b  ");

      // Assert
      _observable.Select(c => c.ContactDisplayName).Should().BeEquivalentTo("Bob Brown");
   }

   [Fact]
   public void ShowFiltered_with_blank_text_restores_the_full_list_after_a_filter()
   {
      // Arrange
      _sut.Reset([Contact("1", "Alice"), Contact("2", "Bob")]);
      _sut.ShowFiltered("alice");
      _observable.Should().ContainSingle();

      // Act
      _sut.ShowFiltered("   ");

      // Assert
      _observable.Should().HaveCount(2);
   }
}
