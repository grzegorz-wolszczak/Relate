using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Relate.ViewModels;

namespace Relate.Pages;

public class BaseContentPage<TViewModel>: ContentPage where TViewModel : BaseVm
{
    protected TViewModel PageViewModel => (TViewModel)BindingContext;
    public BaseContentPage(TViewModel vm)
    {
        // this is from the course, we do not use IOs et all
        On<iOS>().SetUseSafeArea(true);
        BindingContext = vm;
    }
}