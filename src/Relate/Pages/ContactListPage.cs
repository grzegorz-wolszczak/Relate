using System.Diagnostics;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Input;
using Relate.AppLogic;
using Relate.AppLogic.Utils;
using Relate.ViewModels;
using Relate.Views;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Toolkit.NumericEntry;
using Syncfusion.Maui.Toolkit.TabView;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;
using Button = Microsoft.Maui.Controls.Button;
using Grid = Microsoft.Maui.Controls.Grid;
using ItemsView = Microsoft.Maui.Controls.ItemsView;
using StackLayout = Microsoft.Maui.Controls.StackLayout;

namespace Relate.Pages;

using CommunityToolkit.Maui.Markup;

public class ContactListPage : BaseContentPage<ContactListVm>
{
   private readonly ProblemReporterVm _problemReporterVm;

   private enum GenericColumns
   {
      First,
      Second,
      Third
   }

   private enum GenericRows
   {
      First,
      Second,
      Bottom
   }

   private static readonly Color SectionBackgroundColor = SoftPalette.SectionBackground;
   private const int SectionCornerRadius = 16;

   // Held as fields so OnDisappearing can force an in-progress edit to commit
   // (ValueChangeMode is OnLostFocus) before PageViewModel.Save() runs.
   private SfNumericEntry? _properCallEntry;
   private SfNumericEntry? _noCallPeriodEntry;

   public enum SettingsGridRows
   {
      FirstRow,
      SecondRow,
      ThirdRow,
   }

   public enum SettingsGridColumns
   {
      FirstColumn,
      SecondColumn
   }


   private enum ContactListRows
   {
      Header,
      CallList,
      Footer,
      DebugLabelFooter
   }


   public ContactListPage(
      ContactListVm contactListVm, ProblemReporterVm problemReporterVm)
      : base(contactListVm)
   {
      _problemReporterVm = problemReporterVm;
      var tabView = new SfTabView()
      {
         TabBarPlacement = TabBarPlacement.Top,
         IndicatorBackground = new SolidColorBrush(SoftPalette.AccentPrimary),
         IndicatorPlacement = TabIndicatorPlacement.Bottom,
         Items = new()
         {
            new()
            {
               Header = "Contacts",

               //Content = ContactListTabView(),

               /// fresh takes a lot of time !

               Content = new RefreshView() {Content = ContactListTabView()}
                  .Bind(RefreshView.IsRefreshingProperty, getter: (ContactListVm vm) => vm.IsListRefreshing)
                  .Bind(RefreshView.CommandProperty, (ContactListVm vm) => vm.RefreshContactListCommand)
            },
            new() {Header = "Settings", Content = SettingsTabView()},
            new() {Header = "Developer", Content = HelpDeveloperView()}
         }
      };

      // Switching tabs does NOT raise the page's OnDisappearing, so a Settings
      // numeric field still holding focus would keep its just-typed value
      // uncommitted. Force the commit as the selection changes.
      tabView.SelectionChanging += (_, _) =>
      {
         _properCallEntry?.Unfocus();
         _noCallPeriodEntry?.Unfocus();
      };

      Content = tabView;
   }

   private const int FabSize = 56;

   private Grid ContactListTabView()
   {
      return new()
      {
         BackgroundColor = SoftPalette.PageBackground,
         RowDefinitions = Rows.Define(
            (ContactListRows.Header, Auto)
            , (ContactListRows.CallList, Star)
            , (ContactListRows.Footer, Auto)
            , (ContactListRows.DebugLabelFooter, Auto)
         ),
         Children =
         {
            new Border()
               {
                  BackgroundColor = SoftPalette.CardBackground,
                  Stroke = SoftPalette.CardStroke,
                  StrokeThickness = 1,
                  StrokeShape = new RoundRectangle {CornerRadius = new CornerRadius(24)},
                  Shadow = SoftPalette.SoftShadow(),
                  Margin = new Thickness(12, 10, 12, 6),
                  Padding = new Thickness(4, 0),
                  Content = new SearchBar()
                     {
                        BackgroundColor = Colors.Transparent
                     }
                     .Placeholder("search ...")
                     .Center()
                     .Bind(SearchBar.TextProperty,
                        (ContactListVm vm) => vm.SearchBarText,
                        setter: (vm, value) => vm.SearchBarText = value,
                        mode: BindingMode.OneWayToSource)
                     .Bind(SearchBar.IsFocusedProperty,
                        getter: (ContactListVm vm) => vm.IsSearchBarFocused,
                        mode: BindingMode.OneWay)
                     .Behaviors(new UserStoppedTypingBehavior()
                           {
                              StoppedTypingTimeThreshold = 300, ShouldDismissKeyboardAutomatically = false,
                           }
                           .Bind<UserStoppedTypingBehavior, ContactListVm, IRelayCommand<string>>(
                              UserStoppedTypingBehavior.CommandProperty,
                              getter: (ContactListVm vm) => vm.UserStoppedTypingInSearchBarCommand,
                              source: (ContactListVm)this.BindingContext,
                              mode: BindingMode.OneTime
                           ) // end of Behaviour
                     )
               }
               .Row(ContactListRows.Header),
            new CollectionView()
               {
                  Header = null, BackgroundColor = SoftPalette.PageBackground, SelectionMode = SelectionMode.Single
               }
               .Bind(ItemsView.ItemsSourceProperty,
                  getter: (ContactListVm vm) => vm.ObservableContacts)
               .Bind(SelectableItemsView.SelectionChangedCommandProperty,
                  getter: (ContactListVm vm) => vm.SelectionChangedCommand)
               .Bind(SelectableItemsView.SelectedItemProperty,
                  getter: (ContactListVm vm) => vm.SelectedContact,
                  setter: (ContactListVm vm, object? value) => vm.SelectedContact = value)
               .ItemTemplate(new CallLogEntryDataTemplate())
               .Row(ContactListRows.CallList),
            new StackLayout()
            {
               Orientation = StackOrientation.Horizontal,
               HorizontalOptions = LayoutOptions.Center,
               Spacing = 16,
               Children =
               {
                  new ImageButton()
                     {
                        BackgroundColor = SoftPalette.AccentPrimary,
                        Source = ImageSource.FromFile("plus_white_512.png"),
                        CornerRadius = FabSize / 2,
                        Shadow = SoftPalette.SoftShadow(),
                     }
                     .Size(FabSize)
                     .Padding(14)
                     .Margin(5, 8)
                     .Bind(Button.CommandProperty, getter: (ContactListVm vm) => vm.AddContactCommand),
                  new ImageButton()
                     {
                        BackgroundColor = SoftPalette.AccentTeal,
                        Source = ImageSource.FromFile("white_sort_100.png"),
                        CornerRadius = FabSize / 2,
                        Shadow = SoftPalette.SoftShadow(),
                     }
                     .Padding(14)
                     .Margin(5, 8)
                     .Size(FabSize)
                     .Bind(Button.CommandProperty, getter: (ContactListVm vm) => vm.SortByNextCallCommand),
               }
            }.Row(ContactListRows.Footer),
         },
      };
   }

   private View HelpDeveloperView()
   {
      return new Grid
      {
         BindingContext = _problemReporterVm,

         ColumnDefinitions = Columns.Define((GenericColumns.First, Star)),
         RowDefinitions = Rows.Define(
            (GenericRows.First, Auto)
            ,(GenericRows.Second, Star)
            ,(GenericRows.Bottom, 10)
            ),

         Children =
         {
            new Label() { }
               .Row(GenericRows.First)
               .Margin(2)
               .Text($"Version: {BuildInfo.Version}"),


            new Grid()
            {
               ColumnDefinitions = Columns.Define((GenericColumns.First, Star)),
               RowDefinitions = Rows.Define(
                  (GenericRows.First, Auto)
                  ,(GenericRows.Second, Star)
               ),

               Children =
               {
                  new Label()
                     .Text("Logs :")
                     .Margin(2).Row(GenericRows.First),
                  new ScrollView()
                  {
                     BindingContext = _problemReporterVm,
                     Content =
                        new Label()
                           .Margin(2)
                           .Padding(2)
                           .Bind(Label.TextProperty, getter: (ProblemReporterVm vm) => vm.LogContent)
                           .Background(Colors.LightGray),

                  }.Margin(2)
                     .Row(GenericRows.Second)
               }

            }.Row(GenericRows.Second)


         }
      };
   }


   private View SettingsTabView()
   {
      return new ScrollView()
      {
         BackgroundColor = SoftPalette.PageBackground,
         Content = new VerticalStackLayout()
         {
            Spacing = 12,
            Padding = 12,
            Children =
            {
               SettingsSection("Defaults", GetDefaultsGrid()),
               SettingsSection("Backup", GetBackupButtons()),
            }
         }
      };
   }

   private static Border SettingsSection(string title, View content)
   {
      return new Border()
      {
         BackgroundColor = SectionBackgroundColor,
         Stroke = Colors.Transparent,
         StrokeThickness = 0,
         Padding = 16,
         StrokeShape = new RoundRectangle {CornerRadius = new CornerRadius(SectionCornerRadius)},
         Shadow = SoftPalette.SoftShadow(),
         Content = new VerticalStackLayout()
         {
            Spacing = 10,
            Children =
            {
               new Label {CharacterSpacing = 1}
                  .Text(title.ToUpperInvariant())
                  .Font(bold: true, size: 14)
                  .TextColor(SoftPalette.TextPrimary)
                  .TextStart(),
               content
            }
         }
      };
   }

   private Grid GetDefaultsGrid()
   {
      return new Grid()
      {
         ColumnDefinitions = Columns.Define(
            (SettingsGridColumns.FirstColumn, Stars(2))
            , (SettingsGridColumns.SecondColumn, Auto)
         ),
         RowDefinitions = Rows.Define(
            (SettingsGridRows.FirstRow, Auto)
            , (SettingsGridRows.SecondRow, Auto)
            , (SettingsGridRows.ThirdRow, Auto)
         ),
         RowSpacing = 10,
         Children =
         {
            new Label()
               .Margin(2, 2)
               .Padding(2, 2)
               .Text("Min long call minutes:")
               .TextColor(SoftPalette.TextPrimary)
               .TextStart().CenterVertical()
               .FontSize(16)
               .Row(SettingsGridRows.FirstRow)
               .Column(SettingsGridColumns.FirstColumn),

            (_properCallEntry = new SfNumericEntry()
               {
                  ShowBorder = true,
                  FontSize = 20,
                  AllowNull = false,
                  MinimumWidthRequest = 80,
                  Minimum = AppConstants.MinProperCallDurationMinutes,
                  Value = AppConstants.DefaultCallDurationMinutes,
                  Maximum = AppConstants.MaxProperCallDurationMinutes,
                  MaximumNumberDecimalDigits = 0,
                  CustomFormat = "0."
               }.Margin(2, 2)
               .Row(SettingsGridRows.FirstRow)
               .Column(SettingsGridColumns.SecondColumn)
               // classic string-path binding (plain MAUI SetBinding),
               // bypasses CommunityToolkit.Maui.Markup's typed .Bind entirely.
               .Bind(SfNumericEntry.ValueProperty,
                  nameof(ContactListVm.ProperCallDurationMinutes),
                  mode: BindingMode.TwoWay)),


            new Label()
               .Margin(2, 2)
               .Padding(2, 2)
               .Text("No-contact period days:")
               .TextColor(SoftPalette.TextPrimary)
               .TextStart().CenterVertical()
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
                  Value = AppConstants.DefaultNoContactPeriodDays,
                  Maximum = AppConstants.MaxNoContactPeriodDays,
                  MaximumNumberDecimalDigits = 0,
                  CustomFormat = "0."
               }.Margin(2, 2)
               .Row(SettingsGridRows.SecondRow)
               .Column(SettingsGridColumns.SecondColumn)
               .Bind(SfNumericEntry.ValueProperty,
                  nameof(ContactListVm.NoContactPeriodDays),
                  mode: BindingMode.TwoWay)),

            new Label()
               .Margin(2, 2)
               .Padding(2, 2)
               .Text("Aggregate calls by day")
               .TextColor(SoftPalette.TextPrimary)
               .TextStart().CenterVertical()
               .FontSize(16)
               .Row(SettingsGridRows.ThirdRow)
               .Column(SettingsGridColumns.FirstColumn),

            new CheckBox()
               {
                  Color = SoftPalette.AccentPrimary
               }
               .Row(SettingsGridRows.ThirdRow)
               .Column(SettingsGridColumns.SecondColumn)
               // string-path binding + explicit TwoWay: the getter-only lambda .Bind
               // is OneWay, so the user's toggle never propagated back to the VM.
               .Bind(CheckBox.IsCheckedProperty,
                  nameof(ContactListVm.ShouldAggregateConnectionByDay),
                  mode: BindingMode.TwoWay)
         }
      };
   }

   private static View GetBackupButtons()
   {
      return new HorizontalStackLayout()
      {
         Spacing = 12,
         HorizontalOptions = LayoutOptions.Center,
         Children =
         {
            new Button()
               {
                  BackgroundColor = SoftPalette.AccentTeal,
                  TextColor = Colors.White,
                  CornerRadius = 18,
               }
               .Text("Save backup")
               .Bind(Button.CommandProperty, getter: (ContactListVm vm) => vm.CreateContactsBackupCommand),
            new Button()
               {
                  BackgroundColor = SoftPalette.AccentDanger,
                  TextColor = Colors.White,
                  CornerRadius = 18,
               }
               .Text("Restore from backup")
               .Bind(Button.CommandProperty, getter: (ContactListVm vm) => vm.RestoreContactsFromBackupCommand)
         }
      };
   }


   protected override void OnAppearing()
   {
      Trace.WriteLine($"\n*** MAIN PAGE OnAppearing\n");
      base.OnAppearing();
   }

   protected override void OnDisappearing()
   {
      Trace.WriteLine($"\n*** MAIN PAGE OnDisappearing\n");
      base.OnDisappearing();

      // commit any value still being typed in a Settings numeric field
      _properCallEntry?.Unfocus();
      _noCallPeriodEntry?.Unfocus();

      PageViewModel.Save();
   }

   protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
   {
      Trace.WriteLine($"\n*** MAIN PAGE OnNavigatedFrom\n");
      base.OnNavigatedFrom(args);
   }

   protected override void OnNavigatedTo(NavigatedToEventArgs args)
   {
      Trace.WriteLine($"\n*** MAIN PAGE OnNavigatedTo\n");
      base.OnNavigatedTo(args);
   }
}