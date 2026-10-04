using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Relate.AppLogic.Utils;
using Relate.Converters;
using Relate.ViewModels;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;


namespace Relate.Views;

public class CallLogEntryDataTemplate : DataTemplate
{
   private const int AvatarSize = 58;

   private const int ContactTitleFontSize = 16;
   private const int ContactCallFontSize = 13;

   private const int CardCornerRadius = 22;
   private const int StatusDotSize = 9;
   private const int CallIconSize = 40;

   public CallLogEntryDataTemplate() : base(CreateCardTemplate)
   {
   }

   static object CreateCardTemplate()
   {
      return new Border()
      {
         BackgroundColor = SoftPalette.CardBackground,
         Stroke = SoftPalette.CardStroke,
         StrokeThickness = 1,
         StrokeShape = new RoundRectangle
         {
            CornerRadius = new CornerRadius(CardCornerRadius)
         },
         Padding = new Thickness(14, 12),
         Margin = new Thickness(12, 6),

         Content = new Grid()
         {
            ColumnDefinitions = new() {new() {Width = Auto}, new() {Width = Star}, new() {Width = Auto}},
            RowDefinitions = new() {new() {Height = Auto}},

            Children =
            {
               new Button()
                  {
                     Text = "📞",
                     FontSize = 18,
                     TextColor = Colors.White,
                     CornerRadius = CallIconSize / 2,
                     Padding = 0,
                  }
                  .Column(2)
                  .Top()
                  .Size(CallIconSize)
                  .Bind(VisualElement.BackgroundColorProperty,
                     getter: (RelateContactVm vm) => vm.PhoneIconBackgroundColor,
                     mode: BindingMode.OneWay)
                  .Bind(Button.CommandProperty, getter: (RelateContactVm vm) => vm.CallContactCommand),

               new AvatarView()
                  {
                     BorderWidth = 0,
                     WidthRequest = AvatarSize,
                     HeightRequest = AvatarSize,
                     CornerRadius = AvatarSize / 2,
                     Margin = new Thickness(0, 0, 14, 0)
                  }
                  .Column(0)
                  .Center()
                  .Bind(VisualElement.BackgroundColorProperty, getter: (RelateContactVm vm) => vm.ContactImageColor, mode: BindingMode.OneWay)
                  .Bind(AvatarView.ImageSourceProperty, getter: (RelateContactVm entry) => entry.ContactImageSource, mode: BindingMode.OneWay),

               new Grid()
               {
                  ColumnDefinitions = new() {new() {Width = Auto}},
                  RowDefinitions = new()
                  {
                     new() {Height = Auto},
                     new() {Height = Auto},
                     new() {Height = Auto}
                  },
                  RowSpacing = 4,
                  VerticalOptions = LayoutOptions.Center,
                  Children =
                  {
                     new Label()
                        {
                           TextColor = SoftPalette.TextPrimary
                        }
                        .Row(0)
                        .TextStart()
                        .Font(bold: true, size: ContactTitleFontSize)
                        .Bind(Label.TextProperty, getter: (RelateContactVm entry) => entry.ContactDisplayName, mode: BindingMode.OneWay),

                     new Label()
                        {
                           TextColor = SoftPalette.TextSecondary
                        }
                        .Row(1)
                        .TextStart()
                        .Font(size: ContactCallFontSize)
                        .Bind(Label.TextProperty, getter: (RelateContactVm entry) => entry.LastContactCardDisplay),

                     new HorizontalStackLayout()
                     {
                        Spacing = 6,
                        Children =
                        {
                           new Border()
                              {
                                 WidthRequest = StatusDotSize,
                                 HeightRequest = StatusDotSize,
                                 StrokeShape = new Ellipse(),
                                 StrokeThickness = 0
                              }
                              .Center()
                              .Bind(VisualElement.BackgroundColorProperty, nameof(RelateContactVm.NextCallInDays),
                                 converter: new LastCallInDaysToColorConverter()),

                           new Label()
                              .Font(bold: true, size: ContactCallFontSize)
                              .Bind(Label.TextColorProperty, nameof(RelateContactVm.NextCallInDays),
                                 converter: new LastCallInDaysToColorConverter())
                              .Bind(Label.TextProperty, getter: (RelateContactVm entry) => entry.NextCallInDaysDisplay),
                        }
                     }.Row(2)
                  }
               }.Column(1)
            }
         }
      };
   }
}
