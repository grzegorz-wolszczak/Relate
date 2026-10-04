using Relate.AppLogic.CallProcessing;

namespace UnitTests.CallProcessing;

public class NextCallRecalculationTests
{
   [Fact]
   public void CopyValuesTo_writes_both_values_onto_the_view_model()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Build();
      var when = At.Utc(2026, 9, 10, 17);
      var recalculation = new NextCallRecalculation { NextCallDateTime = when, NextCallInDays = 14 };

      // Act
      recalculation.CopyValuesTo(vm);

      // Assert
      vm.NextCallInDays.Should().Be(14);
      vm.NextCallDateTime.Should().Be(when);
   }

   [Fact]
   public void CopyValuesTo_overwrites_previous_values()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Build();
      vm.NextCallInDays = 99;
      var recalculation = new NextCallRecalculation { NextCallDateTime = At.Utc(2026, 9, 1), NextCallInDays = -3 };

      // Act
      recalculation.CopyValuesTo(vm);

      // Assert
      vm.NextCallInDays.Should().Be(-3);
   }

   [Fact]
   public void CopyValuesTo_writes_last_contact_date_and_type()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Build();
      var lastContact = At.Utc(2026, 9, 5);
      var recalculation = new NextCallRecalculation
      {
         NextCallDateTime = At.Utc(2026, 9, 10, 17),
         NextCallInDays = 5,
         LastContactDateTime = lastContact,
         LastContactType = ContactType.Meeting
      };

      // Act
      recalculation.CopyValuesTo(vm);

      // Assert
      vm.LastContactDateTime.Should().Be(lastContact);
      vm.LastContactType.Should().Be(ContactType.Meeting);
   }

   [Fact]
   public void CopyValuesTo_writes_the_last_contact_call_duration()
   {
      // Arrange
      var vm = new RelateContactVmBuilder().Build();
      var recalculation = new NextCallRecalculation
      {
         NextCallDateTime = At.Utc(2026, 9, 10, 17),
         NextCallInDays = 5,
         LastContactDateTime = At.Utc(2026, 9, 5),
         LastContactType = ContactType.Call,
         LastContactCallDuration = TimeSpan.FromMinutes(45)
      };

      // Act
      recalculation.CopyValuesTo(vm);

      // Assert
      vm.LastContactCallDuration.Should().Be(TimeSpan.FromMinutes(45));
   }
}
