using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Relate.AppLogic;
using Relate.AppLogic.Utils;
using Relate.Converters;
using Relate.ViewModels;
using Syncfusion.Maui.Toolkit.NumericEntry;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace Relate.Pages;

public sealed class ContactDetailsPage : BaseContentPage<RelateContactVm>
{
   private enum SettingsGridRows
   {
      FirstRow,
      SecondRow,
      ThirdRow,
   }

   private enum SettingsGridColumns
   {
      FirstColumn,
      SecondColumn
   }

   private enum ContactDetailsTopBarRows
   {
      OnlyOne
   }

   private enum ContactDetailsTopBarColumns
   {
      First, // only to force "squezing"
      Second
   }

   private enum LastMeetingColumns
   {
      Label,
      SetButton,
      ClearButton
   }


   // Held as fields (not constructor-locals) so OnDisappearing can force any
   // in-progress edit to commit before BindingContext is torn down.
   private SfNumericEntry? _longCallEntry;
   private SfNumericEntry? _noCallPeriodEntry;

   protected override void OnDisappearing()
   {
      base.OnDisappearing();

      // ValueChangeMode is OnLostFocus, so a value still being typed hasn't
      // reached the VM yet. Leaving via the "Back to list" button or the Shell
      // back arrow doesn't reliably blur the entry first - force the commit.
      _longCallEntry?.Unfocus();
      _noCallPeriodEntry?.Unfocus();

      // don't hold references any more
      Content = null;
      BindingContext = null;
   }

   //override On

   public ContactDetailsPage(RelateContactVm viewModel)
      : base(viewModel)
   {

      // Shell.SetBackButtonBehavior(this, new BackButtonBehavior()
      // {
      //    TextOverride = "Back",
      // });

      this.Bind(TitleProperty,
         getter: (RelateContactVm vm) => vm.ContactDisplayName);

      BackgroundColor = SoftPalette.PageBackground;

      const int avatarSize = 180;
      const int avatarCornerRadius = avatarSize / 2;
      const int deleteFabSize = 48;

      var detailsStack = new VerticalStackLayout()
         {
            Spacing = 14,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(16, 8, 16, 24),
            Children =
            {
               new Grid()
               {
                  ColumnDefinitions = Columns.Define(
                     (ContactDetailsTopBarColumns.First, Star),
                     (ContactDetailsTopBarColumns.Second, Auto)),

                  RowDefinitions = Rows.Define((ContactDetailsTopBarRows.OnlyOne, Auto)),
                  Children =
                  {
                     new ImageButton()
                        {
                           BackgroundColor = SoftPalette.AccentDanger,
                           Source = ImageSource.FromFile("recycle_bin_white.png"),
                           CornerRadius = deleteFabSize / 2,
                           Shadow = SoftPalette.SoftShadow(),
                        }
                        .Row(ContactDetailsTopBarRows.OnlyOne)
                        .Column(ContactDetailsTopBarColumns.Second)
                        .Size(deleteFabSize)
                        .Padding(12)
                        .Bind(Button.CommandProperty, getter: (RelateContactVm vm) => vm.RemoveContactCommand)
                  }
               },

               new AvatarView()
                  {
                     CornerRadius = avatarCornerRadius,
                     FontAutoScalingEnabled = true,
                     BorderWidth = 4,
                     BorderColor = SoftPalette.CardBackground,
                     Shadow = SoftPalette.SoftShadow(radius: 22, opacity: 0.22f, offsetY: 8)
                  }
                  .Center()
                  .Size(avatarSize)
                  .Bind(VisualElement.BackgroundColorProperty,
                     getter: (RelateContactVm vm) => vm.ContactImageColor
                     , mode: BindingMode.OneWay)
                  .Bind(AvatarView.ImageSourceProperty,
                     getter: (RelateContactVm entry) => entry.ContactImageSource,
                     mode: BindingMode.OneWay
                  ),

               new Label()
                  {
                     TextColor = SoftPalette.TextPrimary
                  }
                  .TextCenter()
                  .Center()
                  .Font(bold: true, size: 28)
                  .Bind(
                     Label.TextProperty,
                     getter: (RelateContactVm vm) => vm.ContactDisplayName),

               new Border()
                  {
                     BackgroundColor = SoftPalette.CardBackground,
                     Stroke = SoftPalette.CardStroke,
                     StrokeThickness = 1,
                     StrokeShape = new RoundRectangle {CornerRadius = new CornerRadius(16)},
                     Padding = new Thickness(14, 8),
                     Content = new VerticalStackLayout()
                        {
                           Spacing = 6,
                           Children =
                           {
                              new Label()
                                 {
                                    TextColor = SoftPalette.TextSecondary
                                 }
                                 .Font(bold: true, size: 14)
                                 .Bind(
                                    Label.TextProperty,
                                    getter: (RelateContactVm vm) => vm.LastContactCardDisplay),
                              new HorizontalStackLayout()
                                 {
                                    Spacing = 6,
                                    Children =
                                    {
                                       new Border()
                                          {
                                             WidthRequest = 9,
                                             HeightRequest = 9,
                                             StrokeShape = new Ellipse(),
                                             StrokeThickness = 0
                                          }
                                          .Center()
                                          .Bind(VisualElement.BackgroundColorProperty, nameof(RelateContactVm.NextCallInDays),
                                             converter: new LastCallInDaysToColorConverter()),
                                       new Label()
                                          .Font(bold: true, size: 14)
                                          .Bind(Label.TextColorProperty
                                             , nameof(RelateContactVm.NextCallInDays)
                                             , converter: new LastCallInDaysToColorConverter()
                                          )
                                          .Bind(Label.TextProperty, getter: (RelateContactVm entry) => entry.NextCallInDaysDisplay),
                                    }
                                 }
                           }
                        }
                  },

               new Border()
                  {
                     BackgroundColor = SoftPalette.CardBackground,
                     Stroke = SoftPalette.CardStroke,
                     StrokeThickness = 1,
                     StrokeShape = new RoundRectangle {CornerRadius = new CornerRadius(16)},
                     Padding = new Thickness(14, 8),
                     Content = new Grid()
                        {
                           ColumnDefinitions = GridRowsColumns.Columns.Define(
                              (SettingsGridColumns.FirstColumn, Stars(2))
                              , (SettingsGridColumns.SecondColumn, Auto)
                           ),
                           RowDefinitions = GridRowsColumns.Rows.Define(
                              (SettingsGridRows.FirstRow, Auto)
                              , (SettingsGridRows.SecondRow, Auto)
                              , (SettingsGridRows.ThirdRow, Auto)
                           ),
                           RowSpacing = 10,
                           Children =
                           {
                              new Label()
                                 {
                                    TextColor = SoftPalette.TextPrimary
                                 }
                                 .Text("Min long call minutes:")
                                 .TextEnd()
                                 .Center()
                                 .TextCenter()
                                 .FontSize(16)
                                 .Row(SettingsGridRows.FirstRow)
                                 .Column(SettingsGridColumns.FirstColumn),
                              (_longCallEntry = new SfNumericEntry()
                                 {
                                    ShowBorder = true,
                                    FontSize = 20,
                                    AllowNull = false,
                                    MinimumWidthRequest = 80,
                                    Minimum = AppConstants.MinProperCallDurationMinutes,
                                    Maximum = AppConstants.MaxProperCallDurationMinutes,
                                    MaximumNumberDecimalDigits = 0,
                                    CustomFormat = "0."
                                 }
                                 .Center()
                                 .Row(SettingsGridRows.FirstRow)
                                 .Column(SettingsGridColumns.SecondColumn)
                                 // classic string-path binding (plain MAUI SetBinding),
                                 // bypasses CommunityToolkit.Maui.Markup's typed .Bind entirely.
                                 .Bind(SfNumericEntry.ValueProperty,
                                    nameof(RelateContactVm.LongCallDurationMinutes),
                                    mode: BindingMode.TwoWay)),
                              new Label()
                                 {
                                    TextColor = SoftPalette.TextPrimary
                                 }
                                 .Text("No-contact period days:")
                                 .TextEnd()
                                 .Center().TextCenter()
                                 .FontSize(16)
                                 .Row(SettingsGridRows.SecondRow)
                                 .Column(SettingsGridColumns.FirstColumn),
                              (_noCallPeriodEntry = new SfNumericEntry()
                                 {
                                    ShowBorder = true,
                                    FontSize = 20,
                                    AllowNull = false,
                                    MinimumWidthRequest = 80,
                                    Minimum = AppConstants.MinNoContactPeriodDays,
                                    Maximum = AppConstants.MaxNoContactPeriodDays,
                                    MaximumNumberDecimalDigits = 0,
                                    CustomFormat = "0."
                                 }
                                 .Center()
                                 .Row(SettingsGridRows.SecondRow)
                                 .Column(SettingsGridColumns.SecondColumn)
                                 // classic string-path binding (plain MAUI SetBinding),
                                 // bypasses CommunityToolkit.Maui.Markup's typed .Bind entirely.
                                 .Bind(SfNumericEntry.ValueProperty,
                                    nameof(RelateContactVm.NoContactPeriodDays),
                                    mode: BindingMode.TwoWay)),
                              new Label()
                                 {
                                    TextColor = SoftPalette.TextPrimary
                                 }
                                 .Text("Aggregate calls by day:")
                                 .TextEnd()
                                 .Center().TextCenter()
                                 .FontSize(16)
                                 .Row(SettingsGridRows.ThirdRow)
                                 .Column(SettingsGridColumns.FirstColumn),
                              new CheckBox()
                                 {
                                    Color = SoftPalette.AccentPrimary
                                 }
                                 .Center()
                                 .Row(SettingsGridRows.ThirdRow)
                                 .Column(SettingsGridColumns.SecondColumn)
                                 .Bind(CheckBox.IsCheckedProperty,
                                    nameof(RelateContactVm.ShouldAggregateConnectionByDay),
                                    mode: BindingMode.TwoWay),
                           }
                        }
                  },

            }
         };

      foreach (var entry in viewModel.ManualContactEntries)
      {
         detailsStack.Children.Add(BuildManualContactCard(entry));
      }

      detailsStack.Children.Add(
         new Button()
            {
               MinimumHeightRequest = 50,
               BackgroundColor = SoftPalette.AccentPrimary,
               CornerRadius = 18,
            }
            .Text("Back to list")
            .Bind(
               Button.CommandProperty,
               getter: (RelateContactVm vm) => vm.HandleBackButtonCommand));

      Content = new ScrollView() {Content = detailsStack};
   }

   // One card per manually-dated contact type (see ManualContactTypeCatalog). BindingContext is
   // the entry itself (not the page's RelateContactVm) so every binding below is a plain property
   // access on ManualContactEntryVm - CommunityToolkit.Maui.Markup's .Bind(getter: ...) can only
   // resolve a simple member-access expression, not a method call closing over `entry`.
   private static View BuildManualContactCard(ManualContactEntryVm entry)
   {
      // Nearly-transparent native DatePicker stacked directly on top of the "SET" button
      // (same grid cell, added last so it sits on top and receives the actual tap).
      // A real user tap on a real DatePicker reliably opens the platform date dialog on
      // every platform; simulating that via .Focus() is not guaranteed to work everywhere.
      var setButton = new Button()
         {
            BackgroundColor = SoftPalette.AccentPrimary,
            CornerRadius = 14,
            FontSize = 13,
            Padding = new Thickness(14, 6),
            InputTransparent = true, // purely decorative backdrop; the DatePicker on top handles taps
         }
         .Text("SET");

      var datePicker = new DatePicker()
         {
            Opacity = 0.02,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            MaximumDate = DateTime.Today
         }
         .Bind(DatePicker.DateProperty,
            getter: (ManualContactEntryVm e) => e.DateOrToday,
            setter: (ManualContactEntryVm e, DateTime value) =>
               e.Date = new DateTimeOffset(value, TimeZoneInfo.Local.GetUtcOffset(value)),
            mode: BindingMode.TwoWay);

      var setOverlay = new Grid()
      {
         Children = {setButton, datePicker}
      };

      return new Border()
         {
            BindingContext = entry,
            BackgroundColor = SoftPalette.CardBackground,
            Stroke = SoftPalette.CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle {CornerRadius = new CornerRadius(16)},
            Padding = new Thickness(14, 8),
            Content = new Grid()
               {
                  ColumnDefinitions = Columns.Define(
                     (LastMeetingColumns.Label, Star)
                     , (LastMeetingColumns.SetButton, Auto)
                     , (LastMeetingColumns.ClearButton, Auto)
                  ),
                  ColumnSpacing = 8,
                  Children =
                  {
                     new Label()
                        {
                           TextColor = SoftPalette.TextSecondary
                        }
                        .Font(bold: true, size: 14)
                        .CenterVertical()
                        .Bind(Label.TextProperty, getter: (ManualContactEntryVm e) => e.SummaryDisplay)
                        .Column(LastMeetingColumns.Label),
                     setOverlay
                        .Column(LastMeetingColumns.SetButton),
                     new Button()
                        {
                           BackgroundColor = SoftPalette.AccentDanger,
                           CornerRadius = 14,
                           FontSize = 13,
                           Padding = new Thickness(14, 6),
                        }
                        .Text("CLEAR")
                        .Column(LastMeetingColumns.ClearButton)
                        .Bind(Button.CommandProperty, getter: (ManualContactEntryVm e) => e.ClearCommand),
                  }
               }
         };
   }
}
