using Relate.ViewModels;

namespace Relate.Pages;

public class BaseContentPage<TViewModel>: ContentPage where TViewModel : BaseVm
{
    protected TViewModel PageViewModel => (TViewModel)BindingContext;
    public BaseContentPage(TViewModel vm)
    {
        BindingContext = vm;
    }
}